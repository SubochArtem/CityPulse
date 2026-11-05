using System.Collections.Concurrent;
using Gateway.Tests.Infrastructure.Constants;
using Microsoft.AspNetCore.Http;

namespace Gateway.Tests.Infrastructure;

public sealed class StubTokenEndpoint : IAsyncDisposable
{
    private static readonly TimeSpan ResponseDelay = TimeSpan.FromMilliseconds(200);

    private readonly ConcurrentDictionary<string, int> _calls = new(StringComparer.Ordinal);
    private StubBackend _backend = null!;

    public string Address => _backend.Address + TestConstants.Bff.TokenPath;

    public static async Task<StubTokenEndpoint> StartAsync()
    {
        var endpoint = new StubTokenEndpoint();
        endpoint._backend = await StubBackend.StartAsync(nameof(StubTokenEndpoint), endpoint.RespondAsync);

        return endpoint;
    }

    public int CallsFor(string refreshToken) => _calls.GetValueOrDefault(refreshToken);

    public static string RefreshedAccessToken(string refreshToken) =>
        TestConstants.Bff.RefreshedAccessTokenPrefix + refreshToken;

    public static string RotatedRefreshToken(string refreshToken) =>
        TestConstants.Bff.RotatedRefreshTokenPrefix + refreshToken;

    private async Task RespondAsync(HttpContext context)
    {
        var form = await context.Request.ReadFormAsync();
        var refreshToken = form["refresh_token"].ToString();
        _calls.AddOrUpdate(refreshToken, 1, (_, count) => count + 1);
        await Task.Delay(ResponseDelay);

        var validClient = form["grant_type"] == "refresh_token"
            && form["client_id"] == TestConstants.Bff.ClientId
            && form["client_secret"] == TestConstants.Bff.ClientSecret;

        if (refreshToken.StartsWith(TestConstants.Bff.FailingRefreshTokenPrefix, StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return;
        }

        if (!validClient || !refreshToken.StartsWith(TestConstants.Bff.ValidRefreshTokenPrefix, StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new Dictionary<string, string> { ["error"] = "invalid_grant" });
            return;
        }

        await context.Response.WriteAsJsonAsync(new Dictionary<string, object>
        {
            ["access_token"] = RefreshedAccessToken(refreshToken),
            ["refresh_token"] = RotatedRefreshToken(refreshToken),
            ["expires_in"] = 3600,
            ["token_type"] = "Bearer"
        });
    }

    public ValueTask DisposeAsync() => _backend.DisposeAsync();
}
