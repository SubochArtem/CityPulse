using System.Threading.RateLimiting;
using Gateway.API.Configuration;
using Gateway.API.Constants;
using Gateway.API.Extensions;
using Gateway.API.Health;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
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

        services.AddOptions<SwaggerSettings>()
            .Bind(configuration.GetSection(SwaggerSettings.SectionName))
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

        services.AddOptions<ServiceHealthSettings>()
            .Bind(configuration.GetSection(ServiceHealthSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient(GatewayDefaults.ServiceHealthClientName)
            .ConfigureHttpClient((serviceProvider, client) =>
                client.Timeout = TimeSpan.FromSeconds(
                    serviceProvider.GetRequiredService<IOptions<ServiceHealthSettings>>().Value.TimeoutSeconds));

        var healthChecks = services.AddHealthChecks();
        foreach (var cluster in GatewayDefaults.ServiceClusters)
        {
            healthChecks.AddTypeActivatedCheck<ServiceReadinessHealthCheck>(
                cluster,
                failureStatus: null,
                tags: [GatewayDefaults.ServicesHealthTag],
                args: [cluster]);
        }

        services.AddGatewayBff(configuration);

        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection(GatewayDefaults.ReverseProxySectionName));

        return services;
    }

    public static void UseGateway(this WebApplication app)
    {
        app.UseForwardedHeaders();
        app.UseCors();
        app.UseAuthentication();
        app.UseRateLimiter();
        app.UseGatewaySwagger();
        app.MapHealthChecks(
            GatewayDefaults.HealthCheckPath,
            new HealthCheckOptions { Predicate = _ => false });
        app.MapHealthChecks(
            GatewayDefaults.ServicesHealthCheckPath,
            new HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains(GatewayDefaults.ServicesHealthTag),
                ResponseWriter = ServicesHealthResponseWriter.WriteAsync
            });
        app.UseGatewayBff();
        app.MapReverseProxy(proxy =>
        {
            proxy.UseGatewayBffTokenForwarding();
            proxy.UseSessionAffinity();
            proxy.UseLoadBalancing();
            proxy.UsePassiveHealthChecks();
        });
    }
}
