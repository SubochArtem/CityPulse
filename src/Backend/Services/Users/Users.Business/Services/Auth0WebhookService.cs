using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Users.Business.Configurations;
using Users.Business.DTOs;
using Users.Business.Exceptions;
using Users.Business.Helpers;
using Users.Business.Interfaces;

namespace Users.Business.Services;

public class Auth0WebhookService(
    IUserService userService,
    IOptions<Auth0Settings> settings) : IIdentityProviderWebhookService
{
    private readonly Auth0Settings _settings = settings.Value;
    private readonly IUserService _userService = userService;

    public async Task HandleAsync(
        string rawBody,
        string signature,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateSignature(rawBody, signature))
            throw new InvalidWebhookSignatureException();

        var payload = JsonSerializer.Deserialize<Auth0WebhookPayload>(rawBody);

        if (payload?.User?.Id is null)
            throw new InvalidWebhookPayloadException("Invalid payload");

        if (!string.Equals(
                payload.Iss,
                UriHelper.BuildHttpsUri(_settings.Domain).ToString(),
                StringComparison.Ordinal))
            throw new InvalidWebhookPayloadException("Invalid issuer");

        if (payload.Event is not IdentityProviderConstants.WebhookUserCreatedEvent)
            return;

        await _userService.CreateUserAsync(new CreateUserDto
        {
            IdentityId = payload.User.Id
        }, cancellationToken);
    }

    private bool ValidateSignature(string body, string signature)
    {
        if (string.IsNullOrWhiteSpace(signature))
            return false;

        if (string.IsNullOrWhiteSpace(_settings.WebhookSecret))
            return false;

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_settings.WebhookSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
        var hashHex = Convert.ToHexString(hash).ToLowerInvariant();
        var expectedSignature = $"{IdentityProviderConstants.WebhookSignaturePrefix}{hashHex}";

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(signature),
            Encoding.UTF8.GetBytes(expectedSignature));
    }
}
