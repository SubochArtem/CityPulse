using Gateway.API.Configuration;
using Gateway.Tests.Infrastructure;
using Gateway.Tests.Infrastructure.Constants;
using Gateway.Tests.Unit.Configuration.TestData;
using Xunit;

namespace Gateway.Tests.Unit.Configuration.Tests;

public sealed class SwaggerSettingsTests
{
    [Fact]
    public void Defaults_ShouldBeDisabled()
    {
        var settings = new SwaggerSettings();

        Assert.False(settings.Enabled);
    }

    [Fact]
    public void Validate_DisabledWithoutValues_ShouldBeValid()
    {
        var errors = SettingsValidator.Validate(new SwaggerSettings());

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_EnabledWithClientIdAndAudience_ShouldBeValid()
    {
        var errors = SettingsValidator.Validate(new SwaggerSettings
        {
            Enabled = true,
            ClientId = TestConstants.Swagger.ClientId,
            Audience = TestConstants.Swagger.Audience
        });

        Assert.Empty(errors);
    }

    [Theory]
    [MemberData(nameof(SwaggerSettingsTestData.BlankValues), MemberType = typeof(SwaggerSettingsTestData))]
    public void Validate_EnabledWithBlankClientId_ShouldBeInvalid(string clientId)
    {
        var errors = SettingsValidator.Validate(new SwaggerSettings
        {
            Enabled = true,
            ClientId = clientId,
            Audience = TestConstants.Swagger.Audience
        });

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(SwaggerSettings.ClientId)));
    }

    [Theory]
    [MemberData(nameof(SwaggerSettingsTestData.BlankValues), MemberType = typeof(SwaggerSettingsTestData))]
    public void Validate_EnabledWithBlankAudience_ShouldBeInvalid(string audience)
    {
        var errors = SettingsValidator.Validate(new SwaggerSettings
        {
            Enabled = true,
            ClientId = TestConstants.Swagger.ClientId,
            Audience = audience
        });

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(SwaggerSettings.Audience)));
    }
}
