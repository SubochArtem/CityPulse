using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Microsoft.Extensions.Options;
using Users.Business.Configurations;
using Users.Business.Exceptions;
using Users.Business.Helpers;
using Users.Business.Interfaces;

namespace Users.Business.Services;

public class Auth0Service(
    IOptions<Auth0Settings> settings,
    IHttpClientFactory httpClientFactory) : IIdentityProvider
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly Auth0Settings _settings = settings.Value;

    private ManagementApiClient? _managementClient;
    private DateTime _tokenExpiresAt = DateTime.MinValue;

    public async Task BlockUserAsync(
        string identityId,
        CancellationToken cancellationToken = default)
    {
        var client = await GetManagementClientAsync(cancellationToken);
        await client.Users.UpdateAsync(
            identityId,
            new UserUpdateRequest { Blocked = true },
            cancellationToken);
    }

    public async Task UnblockUserAsync(
        string identityId,
        CancellationToken cancellationToken = default)
    {
        var client = await GetManagementClientAsync(cancellationToken);
        await client.Users.UpdateAsync(
            identityId,
            new UserUpdateRequest { Blocked = false },
            cancellationToken);
    }

    public async Task DeleteUserAsync(
        string identityId,
        CancellationToken cancellationToken = default)
    {
        var client = await GetManagementClientAsync(cancellationToken);
        await client.Users.DeleteAsync(identityId, cancellationToken);
    }

    private async Task<ManagementApiClient> GetManagementClientAsync(
        CancellationToken cancellationToken)
    {
        if (_managementClient is not null && DateTime.UtcNow < _tokenExpiresAt)
            return _managementClient;

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (_managementClient is not null && DateTime.UtcNow < _tokenExpiresAt)
                return _managementClient;

            var httpClient = _httpClientFactory.CreateClient(
                IdentityProviderConstants.Auth0HttpClientName);

            var authClient = new AuthenticationApiClient(
                UriHelper.BuildHttpsUri(_settings.Domain),
                new HttpClientAuthenticationConnection(httpClient));

            var tokenResponse = await authClient.GetTokenAsync(new ClientCredentialsTokenRequest
            {
                ClientId = _settings.ManagementApiClientId,
                ClientSecret = _settings.ManagementApiClientSecret,
                Audience = _settings.ManagementApiAudience
            }, cancellationToken);

            if (tokenResponse is not { AccessToken: { Length: > 0 } accessToken })
                throw new Auth0Exception("Invalid token response");

            var managementHttpClient = _httpClientFactory.CreateClient(
                IdentityProviderConstants.Auth0HttpClientName);

            _managementClient = new ManagementApiClient(
                accessToken,
                UriHelper.BuildHttpsUri(_settings.Domain, IdentityProviderConstants.Auth0ApiV2Path),
                new HttpClientManagementConnection(managementHttpClient));

            _tokenExpiresAt = DateTime.UtcNow.AddSeconds(
                tokenResponse.ExpiresIn - IdentityProviderConstants.TokenExpiryBufferSeconds);

            return _managementClient;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
