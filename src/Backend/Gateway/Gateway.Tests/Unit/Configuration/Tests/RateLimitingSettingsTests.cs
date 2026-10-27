using Gateway.API.Configuration;
using Gateway.Tests.Infrastructure;
using Gateway.Tests.Unit.Configuration.TestData;
using Xunit;

namespace Gateway.Tests.Unit.Configuration.Tests;

public sealed class RateLimitingSettingsTests
{
    private const int DefaultTokenLimit = 100;
    private const int DefaultTokensPerPeriod = 10;
    private const int DefaultReplenishmentPeriodSeconds = 6;
    private const int DefaultQueueLimit = 0;

    [Fact]
    public void Defaults_ShouldMatchDocumentedValues()
    {
        var settings = new RateLimitingSettings();

        Assert.Equal(DefaultTokenLimit, settings.TokenLimit);
        Assert.Equal(DefaultTokensPerPeriod, settings.TokensPerPeriod);
        Assert.Equal(DefaultReplenishmentPeriodSeconds, settings.ReplenishmentPeriodSeconds);
        Assert.Equal(DefaultQueueLimit, settings.QueueLimit);
    }

    [Fact]
    public void Validate_DefaultSettings_ShouldBeValid()
    {
        var errors = SettingsValidator.Validate(new RateLimitingSettings());

        Assert.Empty(errors);
    }

    [Theory]
    [MemberData(nameof(RateLimitingSettingsTestData.NonPositiveValues), MemberType = typeof(RateLimitingSettingsTestData))]
    public void Validate_TokenLimitIsNotPositive_ShouldBeInvalid(int value)
    {
        var errors = SettingsValidator.Validate(new RateLimitingSettings { TokenLimit = value });

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(RateLimitingSettings.TokenLimit)));
    }

    [Theory]
    [MemberData(nameof(RateLimitingSettingsTestData.NonPositiveValues), MemberType = typeof(RateLimitingSettingsTestData))]
    public void Validate_TokensPerPeriodIsNotPositive_ShouldBeInvalid(int value)
    {
        var errors = SettingsValidator.Validate(new RateLimitingSettings { TokensPerPeriod = value });

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(RateLimitingSettings.TokensPerPeriod)));
    }

    [Theory]
    [MemberData(nameof(RateLimitingSettingsTestData.NonPositiveValues), MemberType = typeof(RateLimitingSettingsTestData))]
    public void Validate_ReplenishmentPeriodIsNotPositive_ShouldBeInvalid(int value)
    {
        var errors = SettingsValidator.Validate(new RateLimitingSettings { ReplenishmentPeriodSeconds = value });

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(RateLimitingSettings.ReplenishmentPeriodSeconds)));
    }

    [Theory]
    [MemberData(nameof(RateLimitingSettingsTestData.NegativeValues), MemberType = typeof(RateLimitingSettingsTestData))]
    public void Validate_QueueLimitIsNegative_ShouldBeInvalid(int value)
    {
        var errors = SettingsValidator.Validate(new RateLimitingSettings { QueueLimit = value });

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(RateLimitingSettings.QueueLimit)));
    }

    [Fact]
    public void Validate_QueueLimitIsZero_ShouldBeValid()
    {
        var errors = SettingsValidator.Validate(new RateLimitingSettings { QueueLimit = 0 });

        Assert.Empty(errors);
    }
}
