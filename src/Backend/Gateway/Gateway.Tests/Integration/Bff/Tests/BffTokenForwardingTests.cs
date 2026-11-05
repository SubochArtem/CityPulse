using System.Net;
using System.Net.Http.Json;
using Gateway.API.Bff;
using Gateway.Tests.Infrastructure;
using Gateway.Tests.Infrastructure.Constants;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Gateway.Tests.Integration.Bff.Tests;

public sealed class BffTokenForwardingTests(GatewayWebAppFactory factory) : GatewayApiTestBase(factory)
{
    private const string ApiPath = "/api/v1/polls";
    private const string ExternalAuthorization = "Bearer swagger-token";
    private const int ValidSeconds = 3600;
    private const int ExpiredSeconds = -10;
    private const int ConcurrentRequests = 5;

    [Fact]
    public async Task Get_Anonymous_ShouldProxyWithoutAuthorization()
    {
        using var client = CreateRawClient();

        var echo = await ReadEchoAsync(await client.GetAsync(ApiPath));

        Assert.Equal(string.Empty, echo.Authorization);
    }

    [Fact]
    public async Task Get_SignedIn_ShouldForwardSessionAccessTokenWithoutCookie()
    {
        using var client = CreateRawClient();
        var cookie = await SignInAsync(client, ValidSeconds);

        var echo = await ReadEchoAsync(await client.SendAsync(CreateApiRequest(HttpMethod.Get, cookie)));

        Assert.Equal($"Bearer {BffTestSignInStartupFilter.CreateAccessToken(
            TestConstants.Bff.PollsReadPermission,
            TestConstants.Bff.IdeasCreatePermission)}", echo.Authorization);
        Assert.Equal(string.Empty, echo.Cookie);
    }

    [Fact]
    public async Task Get_SignedInWithAuthorizationHeader_ShouldKeepExternalAuthorization()
    {
        using var client = CreateRawClient();
        var cookie = await SignInAsync(client, ValidSeconds);
        var request = CreateApiRequest(HttpMethod.Get, cookie);
        request.Headers.TryAddWithoutValidation("Authorization", ExternalAuthorization);

        var echo = await ReadEchoAsync(await client.SendAsync(request));

        Assert.Equal(ExternalAuthorization, echo.Authorization);
    }

    [Fact]
    public async Task Post_SignedInWithoutCsrfHeader_ShouldReturnForbidden()
    {
        using var client = CreateRawClient();
        var cookie = await SignInAsync(client, ValidSeconds);

        var response = await client.SendAsync(CreateApiRequest(HttpMethod.Post, cookie));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Post_SignedInWithCsrfHeader_ShouldForwardSessionAccessToken()
    {
        using var client = CreateRawClient();
        var cookie = await SignInAsync(client, ValidSeconds);
        var request = CreateApiRequest(HttpMethod.Post, cookie);
        request.Headers.Add(TestConstants.Bff.CsrfHeader, TestConstants.Bff.CsrfHeaderValue);

        var response = await client.SendAsync(request);
        var echo = await ReadEchoAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.StartsWith("Bearer ", echo.Authorization);
    }

    [Fact]
    public async Task Post_AnonymousWithoutCsrfHeader_ShouldProxy()
    {
        using var client = CreateRawClient();

        var response = await client.PostAsync(ApiPath, null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_ExpiredToken_ShouldRefreshOnceAndKeepRotatedTokensInSession()
    {
        using var client = CreateRawClient();
        var refreshToken = NewRefreshToken(TestConstants.Bff.ValidRefreshTokenPrefix);
        var cookie = await SignInAsync(client, ExpiredSeconds, refreshToken);

        var first = await ReadEchoAsync(await client.SendAsync(CreateApiRequest(HttpMethod.Get, cookie)));
        var second = await ReadEchoAsync(await client.SendAsync(CreateApiRequest(HttpMethod.Get, cookie)));

        var expected = $"Bearer {StubTokenEndpoint.RefreshedAccessToken(refreshToken)}";
        Assert.Equal(expected, first.Authorization);
        Assert.Equal(expected, second.Authorization);
        Assert.Equal(1, Factory.TokenEndpoint.CallsFor(refreshToken));
        Assert.Equal(0, Factory.TokenEndpoint.CallsFor(StubTokenEndpoint.RotatedRefreshToken(refreshToken)));
    }

    [Fact]
    public async Task Get_ExpiredTokenConcurrently_ShouldRefreshOnlyOnce()
    {
        using var client = CreateRawClient();
        var refreshToken = NewRefreshToken(TestConstants.Bff.ValidRefreshTokenPrefix);
        var cookie = await SignInAsync(client, ExpiredSeconds, refreshToken);

        var responses = await Task.WhenAll(Enumerable.Range(0, ConcurrentRequests)
            .Select(_ => client.SendAsync(CreateApiRequest(HttpMethod.Get, cookie))));
        var echoes = await Task.WhenAll(responses.Select(ReadEchoAsync));

        Assert.All(echoes, echo =>
            Assert.Equal($"Bearer {StubTokenEndpoint.RefreshedAccessToken(refreshToken)}", echo.Authorization));
        Assert.Equal(1, Factory.TokenEndpoint.CallsFor(refreshToken));
    }

    [Fact]
    public async Task Get_RejectedRefreshToken_ShouldEndSessionAndReturnUnauthorized()
    {
        using var client = CreateRawClient();
        var cookie = await SignInAsync(client, ExpiredSeconds, NewRefreshToken("revoked-"));

        var response = await client.SendAsync(CreateApiRequest(HttpMethod.Get, cookie));
        var user = await GetUserAsync(client, cookie);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.False(user.IsAuthenticated);
    }

    [Fact]
    public async Task Get_ExpiredTokenWithoutRefreshToken_ShouldReturnUnauthorized()
    {
        using var client = CreateRawClient();
        var cookie = await SignInAsync(client, ExpiredSeconds);

        var response = await client.SendAsync(CreateApiRequest(HttpMethod.Get, cookie));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_RefreshEndpointFailing_ShouldReturnBadGatewayAndKeepSession()
    {
        using var client = CreateRawClient();
        var cookie = await SignInAsync(client, ExpiredSeconds, NewRefreshToken(TestConstants.Bff.FailingRefreshTokenPrefix));

        var response = await client.SendAsync(CreateApiRequest(HttpMethod.Get, cookie));
        var user = await GetUserAsync(client, cookie);

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.True(user.IsAuthenticated);
    }

    private HttpClient CreateRawClient() =>
        Factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = false });

    private static string NewRefreshToken(string prefix) => prefix + Guid.NewGuid().ToString("N");

    private static async Task<string> SignInAsync(HttpClient client, int expiresInSeconds, string? refreshToken = null)
    {
        var response = await client.GetAsync(BffTestSignInStartupFilter.SignInPath(expiresInSeconds, refreshToken));

        return response.Headers.GetValues(TestConstants.Headers.SetCookie)
            .Select(header => header.Split(';')[0])
            .Single(cookie => cookie.StartsWith(TestConstants.Bff.CookieName + "=", StringComparison.Ordinal));
    }

    private static HttpRequestMessage CreateApiRequest(HttpMethod method, string cookie)
    {
        var request = new HttpRequestMessage(method, ApiPath);
        request.Headers.Add(TestConstants.Headers.Cookie, cookie);

        return request;
    }

    private static async Task<BffUserResponse> GetUserAsync(HttpClient client, string cookie)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, TestConstants.Bff.UserPath);
        request.Headers.Add(TestConstants.Headers.Cookie, cookie);
        var response = await client.SendAsync(request);

        return await response.Content.ReadFromJsonAsync<BffUserResponse>()
            ?? throw new InvalidOperationException("BFF user response was empty.");
    }
}
