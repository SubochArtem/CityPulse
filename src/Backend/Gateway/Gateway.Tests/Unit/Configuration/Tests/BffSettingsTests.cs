using Gateway.API.Configuration;
using Gateway.Tests.Infrastructure;
using Xunit;

namespace Gateway.Tests.Unit.Configuration.Tests;

public sealed class BffSettingsTests
{
    private const string Domain = "tenant.auth0.example";
    private const string ClientId = "client";
    private const string ClientSecret = "secret";
    private const string Audience = "https://api";

    [Fact]
    public void Validate_Disabled_ShouldBeValidWithoutCredentials()
    {
        var errors = SettingsValidator.Validate(new BffSettings());

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_EnabledWithAllValues_ShouldBeValid()
    {
        var errors = SettingsValidator.Validate(CreateEnabled());

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData(nameof(BffSettings.Domain))]
    [InlineData(nameof(BffSettings.ClientId))]
    [InlineData(nameof(BffSettings.ClientSecret))]
    [InlineData(nameof(BffSettings.Audience))]
    public void Validate_EnabledWithMissingValue_ShouldReportThatValue(string member)
    {
        var settings = member switch
        {
            nameof(BffSettings.Domain) => CreateEnabled(domain: string.Empty),
            nameof(BffSettings.ClientId) => CreateEnabled(clientId: string.Empty),
            nameof(BffSettings.ClientSecret) => CreateEnabled(clientSecret: string.Empty),
            _ => CreateEnabled(audience: string.Empty)
        };

        var errors = SettingsValidator.Validate(settings);

        Assert.Contains(errors, e => e.MemberNames.Contains(member));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(366)]
    public void Validate_SessionLifetimeOutOfRange_ShouldBeInvalid(int days)
    {
        var errors = SettingsValidator.Validate(new BffSettings { SessionLifetimeDays = days });

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(BffSettings.SessionLifetimeDays)));
    }

    private static BffSettings CreateEnabled(
        string domain = Domain,
        string clientId = ClientId,
        string clientSecret = ClientSecret,
        string audience = Audience) =>
        new()
        {
            Enabled = true,
            Domain = domain,
            ClientId = clientId,
            ClientSecret = clientSecret,
            Audience = audience
        };
}
