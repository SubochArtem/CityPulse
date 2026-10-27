using CityPulse.Common.Api.Auth;
using CityPulse.Common.Api.Caching;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CityPulse.Common.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCityPulseAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<Auth0Settings>(configuration.GetSection(Auth0Settings.SectionName));

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.ConfigureOptions<ConfigureJwtBearerOptions>();

        return services;
    }

    public static IServiceCollection AddCityPulseRedisCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<RedisSettings>()
            .Bind(configuration.GetSection(RedisSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddStackExchangeRedisCache(_ => { });
        services.AddOptions<RedisCacheOptions>()
            .Configure<IOptions<RedisSettings>>((options, redis) =>
            {
                var settings = redis.Value;
                var configurationOptions = ConfigurationOptions.Parse(settings.ConnectionString);
                configurationOptions.ConnectTimeout = settings.ConnectTimeoutMs;
                configurationOptions.SyncTimeout = settings.SyncTimeoutMs;
                configurationOptions.AsyncTimeout = settings.AsyncTimeoutMs;
                configurationOptions.AbortOnConnectFail = false;
                options.ConfigurationOptions = configurationOptions;
                options.InstanceName = settings.InstanceName;
            });

        services.AddSingleton<ICacheStore, RedisCacheStore>();

        return services;
    }
}
