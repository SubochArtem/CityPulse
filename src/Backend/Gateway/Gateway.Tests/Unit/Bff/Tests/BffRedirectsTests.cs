using Gateway.API.Bff;
using Microsoft.AspNetCore.WebUtilities;
using Xunit;

namespace Gateway.Tests.Unit.Bff.Tests;

public sealed class BffRedirectsTests
{
    private const string Domain = "tenant.auth0.example";
    private const string ClientId = "client";
    private const string ReturnTo = "http://localhost:5173";

    [Fact]
    public void LoginFailed_WithDescription_ShouldCarryErrorAndDescription()
    {
        var url = new Uri("http://host" + BffRedirects.LoginFailed("Access denied & more"));
        var query = QueryHelpers.ParseQuery(url.Query);

        Assert.Equal("/", url.AbsolutePath);
        Assert.Equal("login_failed", query["error"].ToString());
        Assert.Equal("Access denied & more", query["error_description"].ToString());
    }

    [Fact]
    public void LoginFailed_WithoutDescription_ShouldCarryOnlyError()
    {
        Assert.Equal("/?error=login_failed", BffRedirects.LoginFailed(null));
    }

    [Fact]
    public void Logout_ShouldEscapeSessionId()
    {
        Assert.Equal("/bff/logout?sid=a%2Bb", BffRedirects.Logout("a+b"));
    }

    [Fact]
    public void Auth0Logout_ShouldTargetTenantWithClientAndReturnTo()
    {
        var url = new Uri(BffRedirects.Auth0Logout(Domain, ClientId, ReturnTo));
        var query = QueryHelpers.ParseQuery(url.Query);

        Assert.Equal($"https://{Domain}/v2/logout", url.GetLeftPart(UriPartial.Path));
        Assert.Equal(ClientId, query["client_id"].ToString());
        Assert.Equal(ReturnTo, query["returnTo"].ToString());
    }
}
