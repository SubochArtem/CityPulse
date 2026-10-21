using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Users.Business.Configurations;
using Users.Business.DTOs;
using Users.Business.Helpers;
using Users.Tests.Infrastructure.Constants;

namespace Users.Tests.Infrastructure;

public static class Auth0WebhookRequestBuilder
{
    public const string DefaultIdentityId = "auth0|webhook-user-1";
    public const string DefaultNickname = "webhook_nickname";
    public const string DefaultEmail = "webhook.user@example.com";

    public static string BuildPayload(
        string? eventName = IdentityProviderConstants.WebhookUserCreatedEvent,
        string? iss = null,
        string? userId = DefaultIdentityId,
        string? nickname = DefaultNickname,
        string? email = DefaultEmail)
    {
        var payload = new Auth0WebhookPayload
        {
            Event = eventName,
            Iss = iss ?? UriHelper.BuildHttpsUri(EnvironmentConstants.Auth0.Domain).ToString(),
            User = userId is null
                ? null
                : new Auth0WebhookUser
                {
                    Id = userId,
                    Nickname = nickname,
                    Email = email
                }
        };

        return JsonSerializer.Serialize(payload);
    }

    public static string Sign(string body, string secret = EnvironmentConstants.Auth0.WebhookSecret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));

        return $"{IdentityProviderConstants.WebhookSignaturePrefix}{Convert.ToHexString(hash).ToLowerInvariant()}";
    }
}
