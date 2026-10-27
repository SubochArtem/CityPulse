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
    private const string ConnectionStringKey = "Redis:ConnectionString";
    private const string InstanceNameKey = "Redis:InstanceName";
    private const string ConnectTimeoutKey = "Redis:ConnectTimeoutMs";
    private const string SyncTimeoutKey = "Redis:SyncTimeoutMs";
    private const string AsyncTimeoutKey = "Redis:AsyncTimeoutMs";

    private const string ConnectionString = "localhost:6379";
    private const string InstanceName = "test:";
    private const int ConnectTimeoutMs = 300;
    private const int SyncTimeoutMs = 400;
    private const int AsyncTimeoutMs = 600;
    private const int InvalidTimeoutMs = 0;

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
        [ConnectionStringKey] = ConnectionString,
        [InstanceNameKey] = InstanceName,
        [ConnectTimeoutKey] = ConnectTimeoutMs.ToString(),
        [SyncTimeoutKey] = SyncTimeoutMs.ToString(),
        [AsyncTimeoutKey] = AsyncTimeoutMs.ToString()
    };

    private static RedisCacheOptions ResolveRedisCacheOptions(ServiceProvider provider) =>
        provider.GetRequiredService<IOptions<RedisCacheOptions>>().Value;

    private static void AssertValidationFails(ServiceProvider provider) =>
        Assert.Throws<OptionsValidationException>(
            () => provider.GetRequiredService<IOptions<RedisSettings>>().Value);

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
        using var provider = BuildProvider(ValidSettings());

        var options = ResolveRedisCacheOptions(provider);

        Assert.NotNull(options.ConfigurationOptions);
        Assert.Equal(ConnectTimeoutMs, options.ConfigurationOptions.ConnectTimeout);
        Assert.Equal(SyncTimeoutMs, options.ConfigurationOptions.SyncTimeout);
        Assert.Equal(AsyncTimeoutMs, options.ConfigurationOptions.AsyncTimeout);
    }

    [Fact]
    public void AddCityPulseRedisCache_ValidSettings_DoesNotAbortOnConnectFailure()
    {
        using var provider = BuildProvider(ValidSettings());

        var options = ResolveRedisCacheOptions(provider);

        Assert.NotNull(options.ConfigurationOptions);
        Assert.False(options.ConfigurationOptions.AbortOnConnectFail);
    }

    [Fact]
    public void AddCityPulseRedisCache_InstanceNameProvided_AppliesItToRedisOptions()
    {
        using var provider = BuildProvider(ValidSettings());

        var options = ResolveRedisCacheOptions(provider);

        Assert.Equal(InstanceName, options.InstanceName);
    }

    [Theory]
    [InlineData(ConnectionStringKey)]
    [InlineData(InstanceNameKey)]
    [InlineData(ConnectTimeoutKey)]
    [InlineData(SyncTimeoutKey)]
    [InlineData(AsyncTimeoutKey)]
    public void AddCityPulseRedisCache_SettingIsMissing_FailsValidation(string missingKey)
    {
        var settings = ValidSettings();
        settings.Remove(missingKey);
        using var provider = BuildProvider(settings);

        AssertValidationFails(provider);
    }

    [Theory]
    [InlineData(ConnectTimeoutKey)]
    [InlineData(SyncTimeoutKey)]
    [InlineData(AsyncTimeoutKey)]
    public void AddCityPulseRedisCache_TimeoutIsOutOfRange_FailsValidation(string timeoutKey)
    {
        var settings = ValidSettings();
        settings[timeoutKey] = InvalidTimeoutMs.ToString();
        using var provider = BuildProvider(settings);

        AssertValidationFails(provider);
    }
}
