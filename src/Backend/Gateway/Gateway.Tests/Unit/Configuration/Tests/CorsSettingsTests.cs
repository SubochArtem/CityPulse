using Gateway.API.Configuration;
using Gateway.Tests.Infrastructure;
using Xunit;

namespace Gateway.Tests.Unit.Configuration.Tests;

public sealed class CorsSettingsTests
{
    private const string Origin = "http://localhost:5173";

    [Fact]
    public void Validate_NoOrigins_ShouldBeInvalid()
    {
        var errors = SettingsValidator.Validate(new CorsSettings());

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CorsSettings.AllowedOrigins)));
    }

    [Fact]
    public void Validate_WithOrigins_ShouldBeValid()
    {
        var errors = SettingsValidator.Validate(new CorsSettings { AllowedOrigins = [Origin] });

        Assert.Empty(errors);
    }
}
