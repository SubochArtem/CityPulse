using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Users.Business.Exceptions;
using Users.Presentation.Constants;

namespace Users.Presentation.Middleware;

public class GlobalExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlerMiddleware> logger)
{
    private const string ContentType = "application/json";
    private const string ProblemExtensionKeys = "errors";
    private const string ExceptionLogMessage = "Exception occurred";

    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger = logger;
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

            _logger.Log(logLevel, ex, ExceptionLogMessage);
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
                title = ProblemTitles.UserNotFound;
                detail = e.Message;
                break;

            case UserAlreadyExistsException e:
                statusCode = StatusCodes.Status409Conflict;
                title = ProblemTitles.UserAlreadyExists;
                detail = e.Message;
                break;

            case ValidationException e:
                statusCode = StatusCodes.Status400BadRequest;
                title = ProblemTitles.ValidationFailed;
                detail = string.Join("; ", e.Errors.Select(err => err.ErrorMessage));
                break;

            case UnauthorizedAccessException e:
                statusCode = StatusCodes.Status401Unauthorized;
                title = ProblemTitles.Unauthorized;
                detail = e.Message;
                break;

            case InvalidWebhookSignatureException e:
                statusCode = StatusCodes.Status401Unauthorized;
                title = ProblemTitles.Unauthorized;
                detail = e.Message;
                break;

            case InvalidWebhookPayloadException e:
                statusCode = StatusCodes.Status400BadRequest;
                title = ProblemTitles.BadRequest;
                detail = e.Message;
                break;

            case Auth0Exception:
                statusCode = StatusCodes.Status502BadGateway;
                title = ProblemTitles.IdentityProviderError;
                detail = ProblemMessages.IdentityProviderCommunicationError;
                break;

            default:
                statusCode = StatusCodes.Status500InternalServerError;
                title = ProblemTitles.InternalServerError;
                detail = ProblemMessages.UnexpectedError;
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
            problemDetails.Extensions[ProblemExtensionKeys] = validationEx.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(e => e.ErrorMessage).ToList());

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
