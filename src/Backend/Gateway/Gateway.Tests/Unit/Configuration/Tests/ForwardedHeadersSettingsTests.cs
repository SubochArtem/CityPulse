using Gateway.API.Configuration;
using Gateway.Tests.Infrastructure;
using Xunit;

namespace Gateway.Tests.Unit.Configuration.Tests;

public sealed class ForwardedHeadersSettingsTests
{
    [Fact]
    public void Validate_NoNetworks_ShouldBeValid()
    {
        var errors = SettingsValidator.Validate(new ForwardedHeadersSettings());

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("10.0.0.0/8")]
    [InlineData("fd00::/8")]
    public void Validate_CidrNetwork_ShouldBeValid(string network)
    {
        var errors = SettingsValidator.Validate(new ForwardedHeadersSettings { KnownNetworks = [network] });

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("10.0.0.0")]
    [InlineData("not-a-network")]
    [InlineData("10.0.0.0/99")]
    public void Validate_MalformedNetwork_ShouldBeInvalid(string network)
    {
        var errors = SettingsValidator.Validate(new ForwardedHeadersSettings { KnownNetworks = [network] });

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(ForwardedHeadersSettings.KnownNetworks)));
    }
}
