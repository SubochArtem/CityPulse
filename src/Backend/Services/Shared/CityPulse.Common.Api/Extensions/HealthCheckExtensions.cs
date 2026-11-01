using CityPulse.Common.Api.Health;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace CityPulse.Common.Api.Extensions;

public static class HealthCheckExtensions
{
    public static IHealthChecksBuilder AddCityPulseRedisCheck(this IHealthChecksBuilder builder) =>
        builder.AddCheck<RedisHealthCheck>(
            HealthCheckConstants.RedisCheckName,
            tags: [HealthCheckConstants.ReadyTag]);

    public static IEndpointRouteBuilder MapCityPulseHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks(
            HealthCheckConstants.LivePath,
            new HealthCheckOptions
            {
                Predicate = _ => false,
                ResponseWriter = HealthResponseWriter.WriteAsync
            });

        endpoints.MapHealthChecks(
            HealthCheckConstants.ReadyPath,
            new HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains(HealthCheckConstants.ReadyTag),
                ResponseWriter = HealthResponseWriter.WriteAsync
            });

        return endpoints;
    }
}
