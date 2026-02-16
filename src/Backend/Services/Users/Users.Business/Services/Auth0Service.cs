using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Users.Business.Configurations;
using Users.Business.DTOs;
using Users.Business.Exceptions;
using Users.Business.Interfaces;

namespace Users.Business.Services;

public class Auth0Service : IAuth0Service
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<Auth0Service> _logger;
    private readonly Auth0Settings _settings;

    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    private string? _managementApiToken;
    private DateTimeOffset _tokenExpiresAt;

    public Auth0Service(
        HttpClient httpClient,
        IOptions<Auth0Settings> settings,
        ILogger<Auth0Service> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        _httpClient.BaseAddress = new Uri($"https://{_settings.Domain}");
    }

    public Task BlockUserAsync(
        string auth0UserId,
        CancellationToken cancellationToken = default)
    {
        return PatchUserAsync(
            auth0UserId,
            new { blocked = true },
            cancellationToken);
    }

    public Task UnblockUserAsync(
        string auth0UserId,
        CancellationToken cancellationToken = default)
    {
        return PatchUserAsync(
            auth0UserId,
            new { blocked = false },
            cancellationToken);
    }

    public async Task DeleteUserAsync(
        string auth0UserId,
        CancellationToken cancellationToken = default)
    {
        var token = await GetManagementApiTokenAsync(cancellationToken);

        using var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/api/v2/users/{Uri.EscapeDataString(auth0UserId)}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
            await ThrowAuth0ExceptionAsync(
                response,
                $"Failed to delete Auth0 user {auth0UserId}",
                cancellationToken);
    }
    
    private async Task PatchUserAsync(
        string auth0UserId,
        object payload,
        CancellationToken cancellationToken)
    {
        var token = await GetManagementApiTokenAsync(cancellationToken);

        using var request = new HttpRequestMessage(
            HttpMethod.Patch,
            $"/api/v2/users/{Uri.EscapeDataString(auth0UserId)}")
        {
            Content = JsonContent.Create(payload)
        };

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
            await ThrowAuth0ExceptionAsync(
                response,
                $"Failed to update Auth0 user {auth0UserId}",
                cancellationToken);
    }

    private async Task ThrowAuth0ExceptionAsync(
        HttpResponseMessage response,
        string message,
        CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        _logger.LogError(
            "Auth0 request failed. Status={Status}, Body={Body}",
            response.StatusCode,
            body);

        throw new Auth0Exception(
            message,
            response.StatusCode);
    }

    private async Task<string> GetManagementApiTokenAsync(
        CancellationToken cancellationToken)
    {
        if (_managementApiToken != null &&
            DateTimeOffset.UtcNow < _tokenExpiresAt)
            return _managementApiToken;

        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            if (_managementApiToken != null &&
                DateTimeOffset.UtcNow < _tokenExpiresAt)
                return _managementApiToken;

            var request = new Auth0TokenRequest
            {
                ClientId = _settings.ManagementApiClientId,
                ClientSecret = _settings.ManagementApiClientSecret,
                Audience = _settings.ManagementApiAudience,
                GrantType = "client_credentials"
            };

            using var response = await _httpClient.PostAsJsonAsync(
                "/oauth/token",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
                await ThrowAuth0ExceptionAsync(
                    response,
                    "Failed to obtain Auth0 management token",
                    cancellationToken);

            var tokenResponse =
                await response.Content.ReadFromJsonAsync<Auth0TokenResponse>(
                    cancellationToken);

            if (tokenResponse == null ||
                string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
                throw new Auth0Exception("Invalid Auth0 token response");

            _managementApiToken = tokenResponse.AccessToken;
            _tokenExpiresAt =
                DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60);

            return _managementApiToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }
}
