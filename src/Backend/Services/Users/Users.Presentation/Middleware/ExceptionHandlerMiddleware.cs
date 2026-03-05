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
        int statusCode;
        string title;
        string detail;

        switch (ex)
        {
            case UserNotFoundException e:
                statusCode = StatusCodes.Status404NotFound;
                title = UserNotFound;
                detail = e.Message;
                break;

            case UserAlreadyExistsException e:
                statusCode = StatusCodes.Status409Conflict;
                title = UserAlreadyExists;
                detail = e.Message;
                break;

            case ValidationException e:
                statusCode = StatusCodes.Status400BadRequest;
                title = ValidationFailed;
                detail = string.Join("; ", e.Errors.Select(err => err.ErrorMessage));
                break;

            case UnauthorizedAccessException e:
                statusCode = StatusCodes.Status401Unauthorized;
                title = Unauthorized;
                detail = e.Message;
                break;

            case InvalidWebhookSignatureException e:
                statusCode = StatusCodes.Status401Unauthorized;
                title = Unauthorized;
                detail = e.Message;
                break;

            case InvalidWebhookPayloadException e:
                statusCode = StatusCodes.Status400BadRequest;
                title = BadRequest;
                detail = e.Message;
                break;

            case Auth0Exception:
                statusCode = StatusCodes.Status502BadGateway;
                title = IdentityProviderError;
                detail = IdentityProviderCommunicationError;
                break;

            default:
                statusCode = StatusCodes.Status500InternalServerError;
                title = InternalServerError;
                detail = UnexpectedError;
                break;
        }

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
