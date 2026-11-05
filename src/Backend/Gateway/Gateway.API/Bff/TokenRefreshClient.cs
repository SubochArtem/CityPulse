using System.Net.Http.Json;
using System.Text.Json;
using Gateway.API.Configuration;
using Gateway.API.Constants;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;

namespace Gateway.API.Bff;

public sealed class TokenRefreshClient(
    IHttpClientFactory httpClientFactory,
    IOptions<BffSettings> settings,
    IOptionsMonitor<OpenIdConnectOptions> oidcOptions,
    ILogger<TokenRefreshClient> logger)
{
    public async Task<TokenRefreshResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        try
        {
            var tokenEndpoint = await GetTokenEndpointAsync(cancellationToken);
            using var client = httpClientFactory.CreateClient(BffDefaults.TokenClientName);
            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                [BffDefaults.GrantTypeParam] = BffDefaults.RefreshTokenGrant,
                [BffDefaults.ClientIdParam] = settings.Value.ClientId,
                [BffDefaults.ClientSecretParam] = settings.Value.ClientSecret,
                [BffDefaults.RefreshTokenName] = refreshToken
            });
            using var response = await client.PostAsync(tokenEndpoint, content, cancellationToken);

            return await ReadResultAsync(response, cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException or InvalidOperationException
                                              || (exception is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            logger.LogWarning(exception, "Refreshing the session access token failed.");

            return TokenRefreshResult.Failed;
        }
    }

    private async Task<string> GetTokenEndpointAsync(CancellationToken cancellationToken)
    {
        var options = oidcOptions.Get(BffDefaults.OidcScheme);
        var configuration = options.ConfigurationManager is null
            ? options.Configuration
            : await options.ConfigurationManager.GetConfigurationAsync(cancellationToken);

        return configuration?.TokenEndpoint
            ?? throw new InvalidOperationException("The identity provider has no token endpoint.");
    }

    private async Task<TokenRefreshResult> ReadResultAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var statusCode = (int)response.StatusCode;
        if (statusCode is >= 400 and < 500)
        {
            logger.LogInformation("The identity provider rejected the session refresh token with {StatusCode}.", statusCode);

            return TokenRefreshResult.Rejected;
        }

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("The identity provider returned {StatusCode} while refreshing a session.", statusCode);

            return TokenRefreshResult.Failed;
        }

        var tokens = await response.Content.ReadFromJsonAsync<TokenRefreshResponse>(cancellationToken);
        if (string.IsNullOrEmpty(tokens?.AccessToken))
        {
            return TokenRefreshResult.Failed;
        }

        return TokenRefreshResult.Succeeded(tokens);
    }
}
