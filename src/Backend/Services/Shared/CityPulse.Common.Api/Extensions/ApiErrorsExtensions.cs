using CityPulse.Common.Api.Errors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace CityPulse.Common.Api.Extensions;

public static class ApiErrorsExtensions
{
    private const string InvalidValueMessage = "The value is invalid.";
    private const string InvalidBodyMessage = "The request body is invalid.";
    private const string JsonPathPrefix = "$";
    private const string ValidationTitle = "Validation";

    public static IServiceCollection AddUnifiedApiErrors(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressMapClientErrors = true;
            options.InvalidModelStateResponseFactory = CreateInvalidModelStateResponse;
        });

        return services;
    }

    public static IApplicationBuilder UseUnifiedStatusCodePages(this IApplicationBuilder app)
    {
        return app.UseStatusCodePages(WriteStatusCodeProblemAsync);
    }

    private static ObjectResult CreateInvalidModelStateResponse(ActionContext context)
    {
        var errors = context.ModelState
            .SelectMany(entry => entry.Value!.Errors.Select(error => GetMessage(entry.Key, error)))
            .Distinct()
            .DefaultIfEmpty(InvalidValueMessage)
            .ToArray();

        var problemDetails = ProblemDetailsFactory.Create(
            StatusCodes.Status400BadRequest,
            ValidationTitle,
            errors[0],
            context.HttpContext.Request.Path,
            errors);

        return new ObjectResult(problemDetails) { StatusCode = StatusCodes.Status400BadRequest };
    }

    private static string GetMessage(string key, ModelError error)
    {
        if (error.Exception is not null || key.StartsWith(JsonPathPrefix, StringComparison.Ordinal))
            return InvalidBodyMessage;

        return string.IsNullOrWhiteSpace(error.ErrorMessage) ? InvalidValueMessage : error.ErrorMessage;
    }

    private static async Task WriteStatusCodeProblemAsync(StatusCodeContext context)
    {
        var httpContext = context.HttpContext;
        var (title, detail) = StatusProblems.Describe(httpContext.Response.StatusCode);

        var problemDetails = ProblemDetailsFactory.Create(
            httpContext.Response.StatusCode,
            title,
            detail,
            httpContext.Request.Path,
            [detail]);

        await httpContext.Response.WriteAsJsonAsync(problemDetails, httpContext.RequestAborted);
    }
}
