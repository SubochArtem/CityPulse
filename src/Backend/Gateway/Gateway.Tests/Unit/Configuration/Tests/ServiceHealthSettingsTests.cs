using Gateway.API.Configuration;
using Gateway.Tests.Infrastructure;
using Xunit;

namespace Gateway.Tests.Unit.Configuration.Tests;

public sealed class ServiceHealthSettingsTests
{
    private const int DefaultTimeoutSeconds = 4;

    [Fact]
    public void Defaults_ShouldMatchDocumentedValue()
    {
        var settings = new ServiceHealthSettings();

        Assert.Equal(DefaultTimeoutSeconds, settings.TimeoutSeconds);
    }

    [Fact]
    public void Validate_DefaultSettings_ShouldBeValid()
    {
        var errors = SettingsValidator.Validate(new ServiceHealthSettings());

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(61)]
    public void Validate_TimeoutOutOfRange_ShouldBeInvalid(int timeoutSeconds)
    {
        var errors = SettingsValidator.Validate(new ServiceHealthSettings { TimeoutSeconds = timeoutSeconds });

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(ServiceHealthSettings.TimeoutSeconds)));
    }
}
