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
    
    public async Task<GetUserDto?> HandleAsync(
        string rawBody,
        string signature,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateSignature(rawBody, signature))
            throw new InvalidWebhookSignatureException();

        var payload = JsonSerializer.Deserialize<Auth0WebhookPayload>(rawBody);

        if (payload?.User?.Id is null || payload.User.Nickname is null)
            throw new InvalidWebhookPayloadException();

        if (!string.Equals(
                payload.Iss,
                UriHelper.BuildHttpsUri(_settings.Domain).ToString(),
                StringComparison.Ordinal))
            throw new InvalidWebhookPayloadException();

        if (payload.Event is not IdentityProviderConstants.WebhookUserCreatedEvent)
            return null; 

        var existingUser = await _userService.GetUserByIdentityIdAsync(payload.User.Id, cancellationToken);
        
        if (existingUser is not null)
        {
            return existingUser;
        }
        
        var createdUser = await _userService.CreateUserAsync(new CreateUserDto
        {
            IdentityId = payload.User.Id,
            Nickname = payload.User.Nickname
        }, cancellationToken);

        return createdUser;
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
