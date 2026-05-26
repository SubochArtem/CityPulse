using FluentValidation;
using Grpc.Core;
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
    private const string CityNotFound = "City Not Found";
    private const string CityNotActive = "City Not Active";
    private const string CitiesServiceUnavailable = "Cities Service Unavailable";
    private const string CitiesServiceTimeout = "Cities Service Timeout";

    private const string UnexpectedError = "An unexpected error occurred.";
    private const string IdentityProviderCommunicationError = "An error occurred while communicating with the identity provider.";
    private const string CitiesServiceUnavailableError = "Cities service is currently unavailable.";
    private const string CitiesServiceTimeoutError = "Cities service request timed out.";

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
                CityNotFoundException => LogLevel.Warning,
                CityNotActiveException => LogLevel.Warning,
                RpcException => LogLevel.Error,
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

            CityNotFoundException e => (
                StatusCodes.Status404NotFound,
                CityNotFound,
                e.Message
            ),

            CityNotActiveException e => (
                StatusCodes.Status422UnprocessableEntity,
                CityNotActive,
                e.Message
            ),

            RpcException e when e.StatusCode == StatusCode.Unavailable => (
                StatusCodes.Status503ServiceUnavailable,
                CitiesServiceUnavailable,
                CitiesServiceUnavailableError
            ),

            RpcException e when e.StatusCode == StatusCode.DeadlineExceeded => (
                StatusCodes.Status504GatewayTimeout,
                CitiesServiceTimeout,
                CitiesServiceTimeoutError
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
