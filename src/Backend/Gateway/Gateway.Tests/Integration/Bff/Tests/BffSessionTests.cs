using System.Net;
using System.Net.Http.Json;
using Gateway.API.Bff;
using Gateway.Tests.Infrastructure;
using Gateway.Tests.Infrastructure.Constants;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Xunit;

namespace Gateway.Tests.Integration.Bff.Tests;

public sealed class BffSessionTests(GatewayWebAppFactory factory) : GatewayApiTestBase(factory)
{
    private const string ReturnUrl = "/polls/42?tab=ideas";
    private const string UiLocales = "ru";
    private const string WrongSessionId = "not-the-session";

    [Fact]
    public async Task Login_ShouldRedirectToAuth0AuthorizeEndpoint()
    {
        using var client = CreateSessionClient();

        var response = await client.GetAsync($"{TestConstants.Bff.LoginPath}?returnUrl={Uri.EscapeDataString(ReturnUrl)}&ui_locales={UiLocales}");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location!;
        Assert.StartsWith(TestConstants.Bff.AuthorizationEndpoint, location.ToString());
        var query = QueryHelpers.ParseQuery(location.Query);
        Assert.Equal(TestConstants.Bff.ClientId, query["client_id"].ToString());
        Assert.Equal(TestConstants.Bff.Audience, query["audience"].ToString());
        Assert.Equal(TestConstants.Bff.CallbackUri, query["redirect_uri"].ToString());
        Assert.Equal("code", query["response_type"].ToString());
        Assert.Equal(UiLocales, query["ui_locales"].ToString());
        Assert.Contains("offline_access", query["scope"].ToString().Split(' '));
        Assert.False(string.IsNullOrEmpty(query["code_challenge"].ToString()));
    }

    [Fact]
    public async Task User_Anonymous_ShouldReturnUnauthenticatedWithoutCaching()
    {
        using var client = CreateSessionClient();

        var response = await client.GetAsync(TestConstants.Bff.UserPath);
        var user = await response.Content.ReadFromJsonAsync<BffUserResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
        Assert.False(user!.IsAuthenticated);
        Assert.Null(user.InternalUserId);
        Assert.Empty(user.Permissions);
        Assert.Null(user.LogoutUrl);
    }

    [Fact]
    public async Task SignIn_ShouldIssueHttpOnlyLaxCookieWithoutTokens()
    {
        using var client = CreateSessionClient();

        var response = await client.GetAsync(TestConstants.Bff.TestSignInPath);

        var cookie = response.Headers.GetValues("Set-Cookie")
            .Single(header => header.StartsWith(TestConstants.Bff.CookieName + "=", StringComparison.Ordinal));
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.True(cookie.Length < 1024);
    }

    [Fact]
    public async Task User_SignedIn_ShouldReturnUserPermissionsAndLogoutUrl()
    {
        using var client = CreateSessionClient();
        await client.GetAsync(TestConstants.Bff.TestSignInPath);

        var user = await client.GetFromJsonAsync<BffUserResponse>(TestConstants.Bff.UserPath);

        Assert.True(user!.IsAuthenticated);
        Assert.Equal(TestConstants.Bff.UserId, user.UserId);
        Assert.Equal(TestConstants.Bff.InternalUserId, user.InternalUserId);
        Assert.Equal(
            [TestConstants.Bff.PollsReadPermission, TestConstants.Bff.IdeasCreatePermission],
            user.Permissions);
        Assert.StartsWith($"{TestConstants.Bff.LogoutPath}?sid=", user.LogoutUrl);
    }

    [Fact]
    public async Task Logout_Anonymous_ShouldRedirectToRoot()
    {
        using var client = CreateSessionClient();

        var response = await client.GetAsync($"{TestConstants.Bff.LogoutPath}?sid={WrongSessionId}");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Logout_WrongSessionId_ShouldReturnBadRequestAndKeepSession()
    {
        using var client = CreateSessionClient();
        await client.GetAsync(TestConstants.Bff.TestSignInPath);

        var response = await client.GetAsync($"{TestConstants.Bff.LogoutPath}?sid={WrongSessionId}");
        var user = await client.GetFromJsonAsync<BffUserResponse>(TestConstants.Bff.UserPath);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(user!.IsAuthenticated);
    }

    [Fact]
    public async Task Logout_ValidSessionId_ShouldEndSessionAndRedirectToAuth0Logout()
    {
        using var client = CreateSessionClient();
        await client.GetAsync(TestConstants.Bff.TestSignInPath);
        var signedIn = await client.GetFromJsonAsync<BffUserResponse>(TestConstants.Bff.UserPath);

        var response = await client.GetAsync(signedIn!.LogoutUrl);
        var user = await client.GetFromJsonAsync<BffUserResponse>(TestConstants.Bff.UserPath);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location!;
        Assert.StartsWith(TestConstants.Bff.LogoutEndpoint, location.ToString());
        var query = QueryHelpers.ParseQuery(location.Query);
        Assert.Equal(TestConstants.Bff.ClientId, query["client_id"].ToString());
        Assert.Equal(TestConstants.Bff.GatewayOrigin, query["returnTo"].ToString());
        Assert.False(user!.IsAuthenticated);
    }

    private HttpClient CreateSessionClient() =>
        Factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
}
