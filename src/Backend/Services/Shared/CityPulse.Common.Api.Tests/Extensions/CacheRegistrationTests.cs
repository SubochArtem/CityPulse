using CityPulse.Common.Api.Caching;
using CityPulse.Common.Api.Extensions;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace CityPulse.Common.Api.Tests.Extensions;

public sealed class CacheRegistrationTests
{
    private const string ConnectionString = "localhost:6379";
    private const string InstanceName = "test:";

    private static ServiceProvider BuildProvider(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCityPulseRedisCache(configuration);

        return services.BuildServiceProvider();
    }

    private static Dictionary<string, string?> ValidSettings() => new()
    {
        ["Redis:ConnectionString"] = ConnectionString,
        ["Redis:InstanceName"] = InstanceName
    };

    [Fact]
    public void AddCityPulseRedisCache_ValidSettings_ResolvesRedisCacheStore()
    {
        using var provider = BuildProvider(ValidSettings());

        var store = provider.GetRequiredService<ICacheStore>();

        Assert.IsType<RedisCacheStore>(store);
    }

    [Fact]
    public void AddCityPulseRedisCache_StoreResolvedTwice_ReturnsSameInstance()
    {
        using var provider = BuildProvider(ValidSettings());

        var first = provider.GetRequiredService<ICacheStore>();
        var second = provider.GetRequiredService<ICacheStore>();

        Assert.Same(first, second);
    }

    [Fact]
    public void AddCityPulseRedisCache_TimeoutsProvided_AppliesThemToRedisOptions()
    {
        var settings = ValidSettings();
        settings["Redis:ConnectTimeoutMs"] = "300";
        settings["Redis:SyncTimeoutMs"] = "400";
        settings["Redis:AsyncTimeoutMs"] = "600";
        using var provider = BuildProvider(settings);

        var options = provider.GetRequiredService<IOptions<RedisCacheOptions>>().Value;

        Assert.NotNull(options.ConfigurationOptions);
        Assert.Equal(300, options.ConfigurationOptions.ConnectTimeout);
        Assert.Equal(400, options.ConfigurationOptions.SyncTimeout);
        Assert.Equal(600, options.ConfigurationOptions.AsyncTimeout);
    }

    [Fact]
    public void AddCityPulseRedisCache_TimeoutsNotProvided_UsesDefaultOfFiveHundredMilliseconds()
    {
        using var provider = BuildProvider(ValidSettings());

        var options = provider.GetRequiredService<IOptions<RedisCacheOptions>>().Value;

        Assert.NotNull(options.ConfigurationOptions);
        Assert.Equal(500, options.ConfigurationOptions.ConnectTimeout);
        Assert.Equal(500, options.ConfigurationOptions.SyncTimeout);
        Assert.Equal(500, options.ConfigurationOptions.AsyncTimeout);
    }

    [Fact]
    public void AddCityPulseRedisCache_ValidSettings_DoesNotAbortOnConnectFailure()
    {
        using var provider = BuildProvider(ValidSettings());

        var options = provider.GetRequiredService<IOptions<RedisCacheOptions>>().Value;

        Assert.NotNull(options.ConfigurationOptions);
        Assert.False(options.ConfigurationOptions.AbortOnConnectFail);
    }

    [Fact]
    public void AddCityPulseRedisCache_InstanceNameProvided_AppliesItToRedisOptions()
    {
        using var provider = BuildProvider(ValidSettings());

        var options = provider.GetRequiredService<IOptions<RedisCacheOptions>>().Value;

        Assert.Equal(InstanceName, options.InstanceName);
    }

    [Fact]
    public void AddCityPulseRedisCache_ConnectionStringIsMissing_FailsValidation()
    {
        var settings = ValidSettings();
        settings.Remove("Redis:ConnectionString");
        using var provider = BuildProvider(settings);

        Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IOptions<RedisSettings>>().Value);
    }

    [Fact]
    public void AddCityPulseRedisCache_InstanceNameIsMissing_FailsValidation()
    {
        var settings = ValidSettings();
        settings.Remove("Redis:InstanceName");
        using var provider = BuildProvider(settings);

        Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IOptions<RedisSettings>>().Value);
    }

    [Fact]
    public void AddCityPulseRedisCache_TimeoutIsZero_FailsValidation()
    {
        var settings = ValidSettings();
        settings["Redis:SyncTimeoutMs"] = "0";
        using var provider = BuildProvider(settings);

        Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IOptions<RedisSettings>>().Value);
    }
}
