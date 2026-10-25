using Microsoft.AspNetCore.Mvc;

namespace CityPulse.Common.Api.Errors;

public static class ProblemDetailsFactory
{
    private const string ErrorsExtensionKey = "errors";
    private const string CodeExtensionKey = "code";

    public static ProblemDetails Create(
        int statusCode,
        string title,
        string detail,
        string instance,
        IReadOnlyCollection<string> errors,
        string? code = null)
    {
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = instance
        };

        problemDetails.Extensions[ErrorsExtensionKey] = errors.ToArray();

        if (code is not null)
            problemDetails.Extensions[CodeExtensionKey] = code;

        return problemDetails;
    }
}
