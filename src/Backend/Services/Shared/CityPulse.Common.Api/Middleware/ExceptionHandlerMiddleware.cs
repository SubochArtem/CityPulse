using CityPulse.Common.Api.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CityPulse.Common.Api.Middleware;

public class ExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlerMiddleware> logger)
{
    private const string ContentType = "application/json";
    private const string InternalServerErrorTitle = "Internal Server Error";
    private const string InternalServerErrorDetail = "An unexpected error occurred.";

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unhandled exception at {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await HandleExceptionAsync(context);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = ContentType;

        var problemDetails = ProblemDetailsFactory.Create(
            StatusCodes.Status500InternalServerError,
            InternalServerErrorTitle,
            InternalServerErrorDetail,
            context.Request.Path,
            [InternalServerErrorDetail]);

        await context.Response.WriteAsJsonAsync(problemDetails, context.RequestAborted);
    }
}
