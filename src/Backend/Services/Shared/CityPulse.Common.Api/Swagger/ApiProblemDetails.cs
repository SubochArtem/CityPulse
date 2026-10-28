using Microsoft.AspNetCore.Mvc;

namespace CityPulse.Common.Api.Swagger;

public sealed class ApiProblemDetails : ProblemDetails
{
    public string[] Errors { get; init; } = [];

    public string? Code { get; init; }
}
