using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Microsoft.Extensions.Options;
using Users.Business.Configurations;
using Users.Business.Exceptions;
using Users.Business.Interfaces;

namespace Users.Business.Services;

public class Auth0Service : IIdentityProvider
{
    private readonly AuthenticationApiClient _authClient;
    private readonly Lazy<Task<ManagementApiClient>> _managementClient;
    private readonly Auth0Settings _settings;

    public Auth0Service(
        AuthenticationApiClient authClient,
        IOptions<Auth0Settings> settings)
    {
        _authClient = authClient;
        _settings = settings.Value;
        _managementClient = new Lazy<Task<ManagementApiClient>>(CreateManagementClientAsync);
    }

    public async Task BlockUserAsync(
        string identityId,
        CancellationToken cancellationToken = default)
    {
        var client = await _managementClient.Value;
        await client.Users.UpdateAsync(
            identityId,
            new UserUpdateRequest { Blocked = true },
            cancellationToken);
    }

    public async Task UnblockUserAsync(
        string identityId,
        CancellationToken cancellationToken = default)
    {
        var client = await _managementClient.Value;
        await client.Users.UpdateAsync(
            identityId,
            new UserUpdateRequest { Blocked = false },
            cancellationToken);
    }

    public async Task DeleteUserAsync(
        string identityId,
        CancellationToken cancellationToken = default)
    {
        var client = await _managementClient.Value;
        await client.Users.DeleteAsync(identityId, cancellationToken);
    }

    private async Task<ManagementApiClient> CreateManagementClientAsync()
    {
        var tokenResponse = await _authClient.GetTokenAsync(new ClientCredentialsTokenRequest
        {
            ClientId = _settings.ManagementApiClientId,
            ClientSecret = _settings.ManagementApiClientSecret,
            Audience = _settings.ManagementApiAudience
        });

        if (tokenResponse is not { AccessToken: { Length: > 0 } accessToken })
            throw new Auth0Exception("Invalid token response");

        return new ManagementApiClient(accessToken, _settings.Domain);
    }
}
