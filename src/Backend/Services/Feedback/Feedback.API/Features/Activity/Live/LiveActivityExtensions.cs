using CityPulse.Common.Api.Caching;
using StackExchange.Redis;

namespace Feedback.API.Features.Activity.Live;

public static class LiveActivityExtensions
{
    private const string BackplaneChannelPrefix = "feedback:signalr:";

    public static IServiceCollection AddLiveActivity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redis = configuration.GetSection(RedisSettings.SectionName).Get<RedisSettings>()
                    ?? throw new InvalidOperationException(
                        $"Configuration section '{RedisSettings.SectionName}' is missing or invalid.");

        services.AddSignalR()
            .AddStackExchangeRedis(redis.ConnectionString, options =>
            {
                options.Configuration.ChannelPrefix = RedisChannel.Literal(BackplaneChannelPrefix);
                options.Configuration.AbortOnConnectFail = false;
            });

        services.AddSingleton<IActivityNotifier, SignalRActivityNotifier>();

        return services;
    }

    public static void MapLiveActivity(this WebApplication app) =>
        app.MapHub<ActivityHub>(ActivityHub.Route);
}
