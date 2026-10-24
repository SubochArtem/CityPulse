using System.Security.Claims;
using CityPulse.Contracts.Auth.Claims;
using Polls.API.Common.Extensions;
using Polls.Tests.Unit.Extensions.TestData;
using Xunit;

namespace Polls.Tests.Unit.Extensions.Tests;

public sealed class ClaimsPrincipalExtensionsTests
{
    private const string AuthenticationType = "test";

    private static ClaimsPrincipal BuildPrincipal(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, AuthenticationType));

    [Fact]
    public void GetCityId_ClaimContainsGuid_ReturnsIt()
    {
        var cityId = Guid.NewGuid();
        var principal = BuildPrincipal(new Claim(CityPulseClaims.CityId, cityId.ToString()));

        var result = principal.GetCityId();

        Assert.Equal(cityId, result);
    }

    [Fact]
    public void GetCityId_ClaimIsMissing_ReturnsEmptyGuid()
    {
        var principal = BuildPrincipal();

        var result = principal.GetCityId();

        Assert.Equal(Guid.Empty, result);
    }

    [Theory]
    [MemberData(nameof(ClaimsPrincipalExtensionsTestData.InvalidCityIdClaims), MemberType = typeof(ClaimsPrincipalExtensionsTestData))]
    public void GetCityId_ClaimIsNotGuid_ReturnsEmptyGuid(string claimValue)
    {
        var principal = BuildPrincipal(new Claim(CityPulseClaims.CityId, claimValue));

        var result = principal.GetCityId();

        Assert.Equal(Guid.Empty, result);
    }

    [Fact]
    public void GetCityId_PrincipalHasOtherClaimsOnly_ReturnsEmptyGuid()
    {
        var principal = BuildPrincipal(new Claim(ClaimTypes.Name, Guid.NewGuid().ToString()));

        var result = principal.GetCityId();

        Assert.Equal(Guid.Empty, result);
    }
}
