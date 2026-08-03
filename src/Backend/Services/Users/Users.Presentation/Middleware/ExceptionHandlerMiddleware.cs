using FluentValidation;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Users.Business.Exceptions;

namespace Users.Presentation.Middleware;

public class ExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
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
                UnsupportedWebhookEventException => LogLevel.Information,
                CityNotActiveException => LogLevel.Warning,
                RpcException { StatusCode: StatusCode.NotFound } => LogLevel.Warning,
                RpcException => LogLevel.Error,
                _ => LogLevel.Error
            };

            logger.Log(
                logLevel,
                ex,
                MiddlewareConstants.ExceptionLogTemplate,
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
            UnsupportedWebhookEventException e => (
                StatusCodes.Status200OK,
                MiddlewareExceptionMessages.Titles.WebhookEventIgnored,
                e.Message
            ),

            UserNotFoundException e => (
                StatusCodes.Status404NotFound,
                MiddlewareExceptionMessages.Titles.UserNotFound,
                e.Message
            ),

            UserAlreadyExistsException e => (
                StatusCodes.Status409Conflict,
                MiddlewareExceptionMessages.Titles.UserAlreadyExists,
                e.Message
            ),

            ValidationException e => (
                StatusCodes.Status400BadRequest,
                MiddlewareExceptionMessages.Titles.ValidationFailed,
                string.Join("; ", e.Errors.Select(err => err.ErrorMessage))
            ),

            UnauthorizedAccessException e => (
                StatusCodes.Status401Unauthorized,
                MiddlewareExceptionMessages.Titles.Unauthorized,
                e.Message
            ),

            InvalidWebhookSignatureException e => (
                StatusCodes.Status401Unauthorized,
                MiddlewareExceptionMessages.Titles.Unauthorized,
                e.Message
            ),

            InvalidWebhookPayloadException e => (
                StatusCodes.Status400BadRequest,
                MiddlewareExceptionMessages.Titles.BadRequest,
                e.Message
            ),

            Auth0Exception => (
                StatusCodes.Status502BadGateway,
                MiddlewareExceptionMessages.Titles.IdentityProviderError,
                MiddlewareExceptionMessages.Details.IdentityProviderCommunicationError
            ),

            CityNotActiveException e => (
                StatusCodes.Status422UnprocessableEntity,
                MiddlewareExceptionMessages.Titles.CityNotActive,
                e.Message
            ),

            RpcException { StatusCode: StatusCode.NotFound } => (
                StatusCodes.Status404NotFound,
                MiddlewareExceptionMessages.Titles.CityNotFound,
                MiddlewareExceptionMessages.Details.CityNotFound
            ),

            RpcException { StatusCode: StatusCode.Unavailable } => (
                StatusCodes.Status503ServiceUnavailable,
                MiddlewareExceptionMessages.Titles.CitiesServiceUnavailable,
                MiddlewareExceptionMessages.Details.CitiesServiceUnavailable
            ),

            RpcException { StatusCode: StatusCode.DeadlineExceeded } => (
                StatusCodes.Status504GatewayTimeout,
                MiddlewareExceptionMessages.Titles.CitiesServiceTimeout,
                MiddlewareExceptionMessages.Details.CitiesServiceTimeout
            ),

            _ => (
                StatusCodes.Status500InternalServerError,
                MiddlewareExceptionMessages.Titles.InternalServerError,
                MiddlewareExceptionMessages.Details.UnexpectedError
            )
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = MiddlewareConstants.ContentType;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        if (ex is ValidationException validationEx)
            problemDetails.Extensions[MiddlewareConstants.ProblemExtensionKeys] =
                validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToList());

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
