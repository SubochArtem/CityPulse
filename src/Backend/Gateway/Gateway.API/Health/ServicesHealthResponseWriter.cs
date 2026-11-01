using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Gateway.API.Health;

public static class ServicesHealthResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        var services = report.Entries.ToDictionary(
            entry => entry.Key,
            entry => entry.Value.Status.ToString());

        return context.Response.WriteAsJsonAsync(
            new ServicesHealthResponse(report.Status.ToString(), services),
            JsonOptions,
            context.RequestAborted);
    }
}

public sealed record ServicesHealthResponse(string Status, IReadOnlyDictionary<string, string> Services);
