using System.Net;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.Core.Exceptions;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using Microsoft.Extensions.Options;
using Users.Business.Configurations;
using Users.Business.Constants;
using Users.Business.DTOs;
using Users.Business.Exceptions;
using Users.Business.Helpers;
using Users.Business.Interfaces;

namespace Users.Business.Services;

public class Auth0Service(
    IOptions<Auth0Settings> settings,
    IHttpClientFactory httpClientFactory) : IIdentityProvider
{
    private const string TokenRequestFailedMessage = "Auth0 token request failed";
    private const string ManagementRequestFailedMessage = "Auth0 Management API request failed";
    private const string UnreachableMessage = "Auth0 is unreachable";

    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly Auth0Settings _settings = settings.Value;
    private ManagementApiClient? _managementClient;
    private DateTime _tokenExpiresAt = DateTime.MinValue;

    public Task BlockUserAsync(
        string identityId,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(
            identityId,
            client => client.Users.UpdateAsync(
                identityId,
                new UserUpdateRequest { Blocked = true },
                cancellationToken),
            cancellationToken);

    public Task UnblockUserAsync(
        string identityId,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(
            identityId,
            client => client.Users.UpdateAsync(
                identityId,
                new UserUpdateRequest { Blocked = false },
                cancellationToken),
            cancellationToken);

    public Task DeleteUserAsync(
        string identityId,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(
            identityId,
            client => client.Users.DeleteAsync(identityId, cancellationToken),
            cancellationToken);

    public Task UpdateUserProfileAsync(
        string identityId,
        UpdateUserProfileDto updateUserProfileDto,
        CancellationToken cancellationToken = default)
    {
        if (updateUserProfileDto.Nickname is null && updateUserProfileDto.CityId is null)
            return Task.CompletedTask;

        var userUpdateRequest = new UserUpdateRequest();

        if (updateUserProfileDto.Nickname is not null)
            userUpdateRequest.NickName = updateUserProfileDto.Nickname;

        if (updateUserProfileDto.CityId is not null)
            userUpdateRequest.UserMetadata = new { city_id = updateUserProfileDto.CityId.ToString() };

        return ExecuteAsync(
            identityId,
            client => client.Users.UpdateAsync(identityId, userUpdateRequest, cancellationToken),
            cancellationToken);
    }

    private async Task ExecuteAsync(
        string identityId,
        Func<ManagementApiClient, Task> operation,
        CancellationToken cancellationToken)
    {
        var client = await GetManagementClientAsync(cancellationToken);

        try
        {
            await operation(client);
        }
        catch (ErrorApiException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            throw new UserNotFoundException(identityId, IdentitySources.Auth0);
        }
        catch (ErrorApiException exception)
        {
            throw new Auth0Exception(
                $"{ManagementRequestFailedMessage} with status {(int)exception.StatusCode}.",
                exception);
        }
        catch (HttpRequestException exception)
        {
            throw new Auth0Exception(UnreachableMessage, exception);
        }
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

            AccessTokenResponse tokenResponse;
            try
            {
                tokenResponse = await authClient.GetTokenAsync(new ClientCredentialsTokenRequest
                {
                    ClientId = _settings.ManagementApiClientId,
                    ClientSecret = _settings.ManagementApiClientSecret,
                    Audience = _settings.ManagementApiAudience
                }, cancellationToken);
            }
            catch (ErrorApiException exception)
            {
                throw new Auth0Exception(
                    $"{TokenRequestFailedMessage} with status {(int)exception.StatusCode}.",
                    exception);
            }
            catch (HttpRequestException exception)
            {
                throw new Auth0Exception(UnreachableMessage, exception);
            }

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
