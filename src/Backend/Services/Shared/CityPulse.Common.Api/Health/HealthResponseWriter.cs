using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CityPulse.Common.Api.Health;

internal static class HealthResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        var checks = report.Entries.ToDictionary(
            entry => entry.Key,
            entry => entry.Value.Status.ToString());

        return context.Response.WriteAsJsonAsync(
            new HealthResponse(report.Status.ToString(), checks),
            JsonOptions,
            context.RequestAborted);
    }
}

internal sealed record HealthResponse(string Status, IReadOnlyDictionary<string, string> Checks);
