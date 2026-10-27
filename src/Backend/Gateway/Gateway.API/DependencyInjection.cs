using System.Threading.RateLimiting;
using Gateway.API.Configuration;
using Gateway.API.Constants;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Gateway.API;

public static class DependencyInjection
{
    public static IServiceCollection AddGateway(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<CorsSettings>()
            .Bind(configuration.GetSection(CorsSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<RateLimitingSettings>()
            .Bind(configuration.GetSection(RateLimitingSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddCors();
        services.AddOptions<CorsOptions>()
            .Configure<IOptions<CorsSettings>>((options, settings) =>
                options.AddDefaultPolicy(policy => policy
                    .WithOrigins(settings.Value.AllowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()));

        services.AddRateLimiter(_ => { });
        services.AddOptions<RateLimiterOptions>()
            .Configure<IOptions<RateLimitingSettings>>((options, settings) =>
            {
                var limits = settings.Value;
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetTokenBucketLimiter(
                        context.Connection.RemoteIpAddress?.ToString() ?? GatewayDefaults.UnknownClientKey,
                        _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = limits.TokenLimit,
                            TokensPerPeriod = limits.TokensPerPeriod,
                            ReplenishmentPeriod = TimeSpan.FromSeconds(limits.ReplenishmentPeriodSeconds),
                            QueueLimit = limits.QueueLimit
                        }));
            });

        services.AddHealthChecks();

        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection(GatewayDefaults.ReverseProxySectionName));

        return services;
    }

    public static void UseGateway(this WebApplication app)
    {
        app.UseCors();
        app.UseRateLimiter();
        app.MapHealthChecks(GatewayDefaults.HealthCheckPath);
        app.MapReverseProxy();
    }
}
