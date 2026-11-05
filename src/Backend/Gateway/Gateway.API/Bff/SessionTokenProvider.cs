using System.Globalization;
using Gateway.API.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Gateway.API.Bff;

public sealed class SessionTokenProvider(
    SessionKeyReader sessionKeyReader,
    ITicketStore ticketStore,
    SessionLocks sessionLocks,
    TokenRefreshClient refreshClient,
    TimeProvider timeProvider)
{
    public async Task<SessionToken> GetAccessTokenAsync(HttpContext context, AuthenticationProperties properties)
    {
        if (!NeedsRefresh(properties))
        {
            return SessionToken.Available(properties.GetTokenValue(BffDefaults.AccessTokenName)!);
        }

        var sessionKey = sessionKeyReader.Read(context);
        if (sessionKey is null)
        {
            return SessionToken.Expired;
        }

        var sessionLock = sessionLocks.For(sessionKey);
        await sessionLock.WaitAsync(context.RequestAborted);
        try
        {
            return await RefreshAsync(context, sessionKey);
        }
        finally
        {
            sessionLock.Release();
        }
    }

    private async Task<SessionToken> RefreshAsync(HttpContext context, string sessionKey)
    {
        var ticket = await ticketStore.RetrieveAsync(sessionKey, context, context.RequestAborted);
        if (ticket is null)
        {
            return SessionToken.Expired;
        }

        if (NeedsRefresh(ticket.Properties))
        {
            var refreshToken = ticket.Properties.GetTokenValue(BffDefaults.RefreshTokenName);
            if (string.IsNullOrEmpty(refreshToken))
            {
                return SessionToken.Expired;
            }

            var result = await refreshClient.RefreshAsync(refreshToken, context.RequestAborted);
            if (result.Status == TokenRefreshStatus.Rejected)
            {
                return SessionToken.Expired;
            }

            if (result.Status == TokenRefreshStatus.Failed)
            {
                return SessionToken.Unavailable;
            }

            StoreTokens(ticket.Properties, result.Tokens!);
        }

        await context.SignInAsync(BffDefaults.CookieScheme, ticket.Principal, ticket.Properties);

        return SessionToken.Available(ticket.Properties.GetTokenValue(BffDefaults.AccessTokenName)!);
    }

    private bool NeedsRefresh(AuthenticationProperties properties)
    {
        if (string.IsNullOrEmpty(properties.GetTokenValue(BffDefaults.AccessTokenName)))
        {
            return true;
        }

        var expiresAt = properties.GetTokenValue(BffDefaults.ExpiresAtName);

        return DateTimeOffset.TryParse(expiresAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var expiry)
            && expiry - BffDefaults.TokenRefreshSkew <= timeProvider.GetUtcNow();
    }

    private void StoreTokens(AuthenticationProperties properties, TokenRefreshResponse response)
    {
        var tokens = properties.GetTokens().ToDictionary(token => token.Name, token => token.Value);
        tokens[BffDefaults.AccessTokenName] = response.AccessToken!;
        tokens[BffDefaults.ExpiresAtName] = timeProvider.GetUtcNow()
            .AddSeconds(response.ExpiresIn)
            .ToString("o", CultureInfo.InvariantCulture);
        if (!string.IsNullOrEmpty(response.RefreshToken))
        {
            tokens[BffDefaults.RefreshTokenName] = response.RefreshToken;
        }

        if (!string.IsNullOrEmpty(response.IdToken))
        {
            tokens[BffDefaults.IdTokenName] = response.IdToken;
        }

        properties.StoreTokens(tokens.Select(token => new AuthenticationToken { Name = token.Key, Value = token.Value }));
    }
}
