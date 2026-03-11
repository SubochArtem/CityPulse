using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Users.Business.Exceptions;

namespace Users.Presentation.Middleware;

public class ExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlerMiddleware> logger)
{
    private const string ContentType = "application/json";
    private const string ProblemExtensionKeys = "errors";
    private const string ExceptionLogTemplate = "Exception at {Method} {Path}{Query}";

    private const string UserNotFound = "User Not Found";
    private const string UserAlreadyExists = "User Already Exists";
    private const string ValidationFailed = "Validation Failed";
    private const string Unauthorized = "Unauthorized";
    private const string BadRequest = "Bad Request";
    private const string IdentityProviderError = "Identity Provider Error";
    private const string InternalServerError = "Internal Server Error";

    private const string UnexpectedError = "An unexpected error occurred.";
    private const string IdentityProviderCommunicationError = "An error occurred while communicating with the identity provider.";

    private readonly ILogger<ExceptionHandlerMiddleware> _logger = logger;
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var logLevel = ex switch
            {
                UserNotFoundException => LogLevel.Warning,
                UserAlreadyExistsException => LogLevel.Warning,
                ValidationException => LogLevel.Warning,
                InvalidWebhookSignatureException => LogLevel.Warning,
                InvalidWebhookPayloadException => LogLevel.Warning,
                _ => LogLevel.Error
            };

            _logger.Log(
                logLevel,
                ex,
                ExceptionLogTemplate,
                context.Request.Method,
                context.Request.Path,
                context.Request.QueryString);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, title, detail) = ex switch
        {
            UserNotFoundException e => (
                StatusCodes.Status404NotFound,
                UserNotFound,
                e.Message
            ),

            UserAlreadyExistsException e => (
                StatusCodes.Status409Conflict,
                UserAlreadyExists,
                e.Message
            ),

            ValidationException e => (
                StatusCodes.Status400BadRequest,
                ValidationFailed,
                string.Join("; ", e.Errors.Select(err => err.ErrorMessage))
            ),

            UnauthorizedAccessException e => (
                StatusCodes.Status401Unauthorized,
                Unauthorized,
                e.Message
            ),

            InvalidWebhookSignatureException e => (
                StatusCodes.Status401Unauthorized,
                Unauthorized,
                e.Message
            ),

            InvalidWebhookPayloadException e => (
                StatusCodes.Status400BadRequest,
                BadRequest,
                e.Message
            ),

            Auth0Exception => (
                StatusCodes.Status502BadGateway,
                IdentityProviderError,
                IdentityProviderCommunicationError
            ),

            _ => (
                StatusCodes.Status500InternalServerError,
                InternalServerError,
                UnexpectedError
            )
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = ContentType;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        if (ex is ValidationException validationEx)
            problemDetails.Extensions[ProblemExtensionKeys] =
                validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToList());

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
