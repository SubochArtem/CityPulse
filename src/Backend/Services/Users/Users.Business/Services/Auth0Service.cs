using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Microsoft.Extensions.Options;
using Users.Business.Configurations;
using Users.Business.Exceptions;
using Users.Business.Interfaces;

namespace Users.Business.Services;

public class Auth0Service(
    AuthenticationApiClient authClient,
    IOptions<Auth0Settings> settings) : IIdentityProvider
{
    private readonly AuthenticationApiClient _authClient = authClient;
    private readonly Auth0Settings _settings = settings.Value;

    public Task BlockUserAsync(
        string identityId,
        CancellationToken cancellationToken = default)
    {
        return PatchUserAsync(
            identityId,
            new UserUpdateRequest { Blocked = true },
            cancellationToken);
    }

    public Task UnblockUserAsync(
        string identityId,
        CancellationToken cancellationToken = default)
    {
        return PatchUserAsync(
            identityId,
            new UserUpdateRequest { Blocked = false },
            cancellationToken);
    }

    public async Task DeleteUserAsync(
        string identityId,
        CancellationToken cancellationToken = default)
    {
        var client = await CreateManagementClientAsync(cancellationToken);
        await client.Users.DeleteAsync(identityId, cancellationToken);
    }

    private async Task PatchUserAsync(
        string identityId,
        UserUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var client = await CreateManagementClientAsync(cancellationToken);
        await client.Users.UpdateAsync(identityId, request, cancellationToken);
    }

    private async Task<ManagementApiClient> CreateManagementClientAsync(
        CancellationToken cancellationToken)
    {
        var token = await GetManagementApiTokenAsync(cancellationToken);
        return new ManagementApiClient(token, _settings.Domain);
    }

    private async Task<string> GetManagementApiTokenAsync(
        CancellationToken cancellationToken)
    {
        var tokenResponse = await _authClient.GetTokenAsync(
            new ClientCredentialsTokenRequest
            {
                ClientId = _settings.ManagementApiClientId,
                ClientSecret = _settings.ManagementApiClientSecret,
                Audience = _settings.ManagementApiAudience
            },
            cancellationToken);

        if (tokenResponse is not { AccessToken: { Length: > 0 } accessToken })
            throw new Auth0Exception("Invalid token response");

        return accessToken;
    }
}
