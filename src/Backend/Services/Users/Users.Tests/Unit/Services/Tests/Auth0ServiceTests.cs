using System.Net;
using System.Text.Json;
using System.Web;
using Auth0.Core.Exceptions;
using Microsoft.Extensions.Options;
using Moq;
using Users.Business.Configurations;
using Users.Business.Constants;
using Users.Business.DTOs;
using Users.Business.Exceptions;
using Users.Business.Services;
using Users.Tests.Infrastructure;
using Users.Tests.Unit.Services.TestData;
using Xunit;

namespace Users.Tests.Unit.Services.Tests;

public sealed class Auth0ServiceTests
{
    private const string Domain = "tenant.example.com";
    private const string IdentityId = "auth0|abc123";
    private const string AccessToken = "test-access-token";
    private const string TokenPath = "/oauth/token";
    private const string UserPath = "/api/v2/users/auth0|abc123";
    private const int LongLivedTokenSeconds = 3600;
    private const int ExpiredTokenSeconds = IdentityProviderConstants.TokenExpiryBufferSeconds;
    private const string Nickname = "new_nickname";
    private const string ExpectedAuthorization = $"Bearer {AccessToken}";
    private const string EmptyJsonObject = "{}";
    private const string InvalidTokenResponseMessage = "Invalid token response";
    private const string UnreachableMessage = "Auth0 is unreachable";
    private const string ManagementFailedMessage = "Auth0 Management API request failed with status 500.";
    private const string TokenFailedMessagePrefix = "Auth0 token request failed with status";
    private const string ClientCredentialsGrantType = "client_credentials";

    private const string BlockedField = "blocked";
    private const string NicknameField = "nickname";
    private const string UserMetadataField = "user_metadata";
    private const string CityIdField = "city_id";
    private const string GrantTypeField = "grant_type";
    private const string ClientIdField = "client_id";
    private const string ClientSecretField = "client_secret";
    private const string AudienceField = "audience";

    private const string ClientId = "client-id";
    private const string ClientSecret = "client-secret";
    private const string Audience = "audience";
    private const string ManagementApiClientId = "mgmt-client-id";
    private const string ManagementApiClientSecret = "mgmt-client-secret";
    private const string ManagementApiAudience = "mgmt-audience";
    private const string WebhookSecret = "webhook-secret";

    private readonly Mock<IHttpClientFactory> _httpClientFactory = new(MockBehavior.Strict);
    private readonly Auth0Settings _settings = BuildSettings();
    private int _tokenExpiresIn = LongLivedTokenSeconds;
    private string _accessToken = AccessToken;
    private HttpStatusCode _tokenStatusCode = HttpStatusCode.OK;
    private HttpStatusCode _managementStatusCode = HttpStatusCode.OK;
    private Exception? _tokenException;
    private Exception? _managementException;
    private readonly StubHttpMessageHandler _handler;
    private readonly Auth0Service _sut;

    public Auth0ServiceTests()
    {
        _handler = new StubHttpMessageHandler(Respond);

        _httpClientFactory
            .Setup(f => f.CreateClient(IdentityProviderConstants.Auth0HttpClientName))
            .Returns(() => new HttpClient(_handler, disposeHandler: false));

        _sut = new Auth0Service(Options.Create(_settings), _httpClientFactory.Object);
    }

    private static Auth0Settings BuildSettings() => new()
    {
        Domain = Domain,
        ClientId = ClientId,
        ClientSecret = ClientSecret,
        Audience = Audience,
        ManagementApiClientId = ManagementApiClientId,
        ManagementApiClientSecret = ManagementApiClientSecret,
        ManagementApiAudience = ManagementApiAudience,
        WebhookSecret = WebhookSecret
    };

    private HttpResponseMessage Respond(RecordedRequest request)
    {
        if (request.Uri!.AbsolutePath == TokenPath)
        {
            if (_tokenException is not null)
                throw _tokenException;

            var tokenJson = JsonSerializer.Serialize(new
            {
                access_token = _accessToken,
                expires_in = _tokenExpiresIn
            });

            return new HttpResponseMessage(_tokenStatusCode)
            {
                Content = new StringContent(tokenJson)
            };
        }

        if (_managementException is not null)
            throw _managementException;

        return new HttpResponseMessage(_managementStatusCode)
        {
            Content = new StringContent(EmptyJsonObject)
        };
    }

    private IEnumerable<RecordedRequest> TokenRequests =>
        _handler.Requests.Where(r => r.Uri!.AbsolutePath == TokenPath);

    private IEnumerable<RecordedRequest> ManagementRequests =>
        _handler.Requests.Where(r => r.Uri!.AbsolutePath != TokenPath);

    private static string DecodedPath(RecordedRequest request) =>
        Uri.UnescapeDataString(request.Uri!.AbsolutePath);

    [Fact]
    public async Task BlockUserAsync_ValidIdentityId_SendsPatchWithBlockedTrue()
    {
        await _sut.BlockUserAsync(IdentityId);

        var request = Assert.Single(ManagementRequests);
        Assert.Equal(HttpMethod.Patch, request.Method);
        Assert.Equal(UserPath, DecodedPath(request));
        Assert.Equal(ExpectedAuthorization, request.Authorization);
        using var body = JsonDocument.Parse(request.Body!);
        Assert.True(body.RootElement.GetProperty(BlockedField).GetBoolean());
    }

    [Fact]
    public async Task UnblockUserAsync_ValidIdentityId_SendsPatchWithBlockedFalse()
    {
        await _sut.UnblockUserAsync(IdentityId);

        var request = Assert.Single(ManagementRequests);
        Assert.Equal(HttpMethod.Patch, request.Method);
        Assert.Equal(UserPath, DecodedPath(request));
        using var body = JsonDocument.Parse(request.Body!);
        Assert.False(body.RootElement.GetProperty(BlockedField).GetBoolean());
    }

    [Fact]
    public async Task DeleteUserAsync_ValidIdentityId_SendsDeleteRequest()
    {
        _managementStatusCode = HttpStatusCode.NoContent;

        await _sut.DeleteUserAsync(IdentityId);

        var request = Assert.Single(ManagementRequests);
        Assert.Equal(HttpMethod.Delete, request.Method);
        Assert.Equal(UserPath, DecodedPath(request));
        Assert.Equal(ExpectedAuthorization, request.Authorization);
    }

    [Fact]
    public async Task UpdateUserProfileAsync_NothingToUpdate_SendsNoRequests()
    {
        await _sut.UpdateUserProfileAsync(IdentityId, new UpdateUserProfileDto());

        Assert.Empty(_handler.Requests);
    }

    [Fact]
    public async Task UpdateUserProfileAsync_NicknameOnly_SendsNicknameWithoutMetadata()
    {
        await _sut.UpdateUserProfileAsync(IdentityId, new UpdateUserProfileDto { Nickname = Nickname });

        var request = Assert.Single(ManagementRequests);
        Assert.Equal(HttpMethod.Patch, request.Method);
        using var body = JsonDocument.Parse(request.Body!);
        Assert.Equal(Nickname, body.RootElement.GetProperty(NicknameField).GetString());
        Assert.False(body.RootElement.TryGetProperty(UserMetadataField, out _));
    }

    [Fact]
    public async Task UpdateUserProfileAsync_CityOnly_SendsCityIdMetadataWithoutNickname()
    {
        var cityId = Guid.NewGuid();

        await _sut.UpdateUserProfileAsync(IdentityId, new UpdateUserProfileDto { CityId = cityId });

        var request = Assert.Single(ManagementRequests);
        using var body = JsonDocument.Parse(request.Body!);
        var metadata = body.RootElement.GetProperty(UserMetadataField);
        Assert.Equal(cityId.ToString(), metadata.GetProperty(CityIdField).GetString());
        Assert.False(body.RootElement.TryGetProperty(NicknameField, out _));
    }

    [Fact]
    public async Task UpdateUserProfileAsync_NicknameAndCity_SendsBothFields()
    {
        var cityId = Guid.NewGuid();

        await _sut.UpdateUserProfileAsync(
            IdentityId,
            new UpdateUserProfileDto { Nickname = Nickname, CityId = cityId });

        var request = Assert.Single(ManagementRequests);
        using var body = JsonDocument.Parse(request.Body!);
        Assert.Equal(Nickname, body.RootElement.GetProperty(NicknameField).GetString());
        Assert.Equal(
            cityId.ToString(),
            body.RootElement.GetProperty(UserMetadataField).GetProperty(CityIdField).GetString());
    }

    [Fact]
    public async Task BlockUserAsync_RequestsTokenWithConfiguredCredentials()
    {
        await _sut.BlockUserAsync(IdentityId);

        var request = Assert.Single(TokenRequests);
        Assert.Equal(HttpMethod.Post, request.Method);
        var form = HttpUtility.ParseQueryString(request.Body!);
        Assert.Equal(ClientCredentialsGrantType, form[GrantTypeField]);
        Assert.Equal(ManagementApiClientId, form[ClientIdField]);
        Assert.Equal(ManagementApiClientSecret, form[ClientSecretField]);
        Assert.Equal(ManagementApiAudience, form[AudienceField]);
    }

    [Fact]
    public async Task BlockUserAsync_CalledTwiceWithValidToken_RequestsTokenOnce()
    {
        await _sut.BlockUserAsync(IdentityId);
        await _sut.UnblockUserAsync(IdentityId);

        Assert.Single(TokenRequests);
        Assert.Equal(2, ManagementRequests.Count());
    }

    [Fact]
    public async Task BlockUserAsync_CalledTwiceWithExpiredToken_RequestsTokenAgain()
    {
        _tokenExpiresIn = ExpiredTokenSeconds;

        await _sut.BlockUserAsync(IdentityId);
        await _sut.UnblockUserAsync(IdentityId);

        Assert.Equal(2, TokenRequests.Count());
        Assert.Equal(2, ManagementRequests.Count());
    }

    [Fact]
    public async Task BlockUserAsync_EmptyAccessToken_ThrowsAuth0ExceptionAndSendsNoManagementRequest()
    {
        _accessToken = string.Empty;

        var exception = await Assert.ThrowsAsync<Auth0Exception>(() => _sut.BlockUserAsync(IdentityId));

        Assert.Equal(InvalidTokenResponseMessage, exception.Message);
        Assert.Empty(ManagementRequests);
    }

    [Theory]
    [MemberData(nameof(Auth0ServiceTestData.TokenErrorStatusCodes), MemberType = typeof(Auth0ServiceTestData))]
    public async Task BlockUserAsync_TokenEndpointFails_ThrowsAuth0ExceptionAndSendsNoManagementRequest(
        HttpStatusCode tokenStatusCode)
    {
        _tokenStatusCode = tokenStatusCode;

        var exception = await Assert.ThrowsAsync<Auth0Exception>(() => _sut.BlockUserAsync(IdentityId));

        Assert.StartsWith(TokenFailedMessagePrefix, exception.Message);
        Assert.IsAssignableFrom<ErrorApiException>(exception.InnerException);
        Assert.Empty(ManagementRequests);
    }

    [Fact]
    public async Task BlockUserAsync_TokenEndpointUnreachable_ThrowsAuth0ExceptionAndSendsNoManagementRequest()
    {
        _tokenException = new HttpRequestException();

        var exception = await Assert.ThrowsAsync<Auth0Exception>(() => _sut.BlockUserAsync(IdentityId));

        Assert.Equal(UnreachableMessage, exception.Message);
        Assert.IsType<HttpRequestException>(exception.InnerException);
        Assert.Empty(ManagementRequests);
    }

    [Theory]
    [MemberData(nameof(Auth0ServiceTestData.Operations), MemberType = typeof(Auth0ServiceTestData))]
    public async Task Operation_UserMissingInAuth0_ThrowsUserNotFoundException(
        Func<Auth0Service, string, Task> operation)
    {
        _managementStatusCode = HttpStatusCode.NotFound;

        var exception = await Assert.ThrowsAsync<UserNotFoundException>(() => operation(_sut, IdentityId));

        Assert.Equal(IdentityId, exception.IdentityId);
        Assert.Equal(IdentitySources.Auth0, exception.IdentityProvider);
    }

    [Theory]
    [MemberData(nameof(Auth0ServiceTestData.Operations), MemberType = typeof(Auth0ServiceTestData))]
    public async Task Operation_ManagementApiFails_ThrowsAuth0ExceptionWithStatus(
        Func<Auth0Service, string, Task> operation)
    {
        _managementStatusCode = HttpStatusCode.InternalServerError;

        var exception = await Assert.ThrowsAsync<Auth0Exception>(() => operation(_sut, IdentityId));

        Assert.Equal(ManagementFailedMessage, exception.Message);
        Assert.IsAssignableFrom<ErrorApiException>(exception.InnerException);
    }

    [Theory]
    [MemberData(nameof(Auth0ServiceTestData.ManagementErrorStatusCodes), MemberType = typeof(Auth0ServiceTestData))]
    public async Task BlockUserAsync_ManagementApiReturnsErrorStatus_ThrowsAuth0Exception(
        HttpStatusCode managementStatusCode)
    {
        _managementStatusCode = managementStatusCode;

        var exception = await Assert.ThrowsAsync<Auth0Exception>(() => _sut.BlockUserAsync(IdentityId));

        Assert.IsAssignableFrom<ErrorApiException>(exception.InnerException);
    }

    [Theory]
    [MemberData(nameof(Auth0ServiceTestData.Operations), MemberType = typeof(Auth0ServiceTestData))]
    public async Task Operation_ManagementApiUnreachable_ThrowsAuth0Exception(
        Func<Auth0Service, string, Task> operation)
    {
        _managementException = new HttpRequestException();

        var exception = await Assert.ThrowsAsync<Auth0Exception>(() => operation(_sut, IdentityId));

        Assert.Equal(UnreachableMessage, exception.Message);
        Assert.IsType<HttpRequestException>(exception.InnerException);
    }
}
