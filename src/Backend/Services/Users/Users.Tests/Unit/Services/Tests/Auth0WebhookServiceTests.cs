using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Moq;
using Users.Business.Configurations;
using Users.Business.DTOs;
using Users.Business.Exceptions;
using Users.Business.Helpers;
using Users.Business.Interfaces;
using Users.Business.Services;
using Users.Tests.Unit.Services.TestData;
using Xunit;

namespace Users.Tests.Unit.Services.Tests;

public sealed class Auth0WebhookServiceTests
{
    private const string Domain = "tenant.example.com";
    private const string WebhookSecret = "test-webhook-secret";
    private const string DefaultIdentityId = "auth0|abc123";
    private const string DefaultNickname = "some_nickname";
    private const string ProvidedNickname = "provided_nickname";
    private const string DefaultEmail = "someone@example.com";
    private const string EmailLocalPart = "someone";
    private const string IrrelevantReturnedNickname = "irrelevant_returned_nickname";

    private readonly Mock<IUserService> _userService = new(MockBehavior.Strict);
    private readonly Auth0Settings _settings;
    private readonly Auth0WebhookService _sut;

    public Auth0WebhookServiceTests()
    {
        _settings = BuildSettings();
        _sut = new Auth0WebhookService(_userService.Object, Options.Create(_settings));
    }

    private static Auth0Settings BuildSettings(string webhookSecret = WebhookSecret) => new()
    {
        Domain = Domain,
        ClientId = "client-id",
        ClientSecret = "client-secret",
        Audience = "audience",
        ManagementApiClientId = "mgmt-client-id",
        ManagementApiClientSecret = "mgmt-client-secret",
        ManagementApiAudience = "mgmt-audience",
        WebhookSecret = webhookSecret
    };

    private static string BuildPayloadJson(
        string? eventName = IdentityProviderConstants.WebhookUserCreatedEvent,
        string? iss = null,
        string? userId = DefaultIdentityId,
        string? nickname = DefaultNickname,
        string? email = DefaultEmail)
    {
        var payload = new Auth0WebhookPayload
        {
            Event = eventName,
            Iss = iss ?? UriHelper.BuildHttpsUri(Domain).ToString(),
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

    private static string ComputeValidSignature(string body, string secret = WebhookSecret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
        var hashHex = Convert.ToHexString(hash).ToLowerInvariant();
        return $"{IdentityProviderConstants.WebhookSignaturePrefix}{hashHex}";
    }

    private static GetUserDto BuildIrrelevantCreatedUserDto(string nickname = IrrelevantReturnedNickname) => new()
    {
        Id = Guid.NewGuid(),
        Nickname = nickname,
        CityId = null,
        IdentityId = DefaultIdentityId,
        AccessStatus = 0,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = null
    };

    [Theory]
    [MemberData(nameof(Auth0WebhookServiceTestData.EmptyOrNullSignatures), MemberType = typeof(Auth0WebhookServiceTestData))]
    public async Task HandleAsync_SignatureIsNullOrWhitespace_ThrowsInvalidWebhookSignatureException(string? signature)
    {
        var body = BuildPayloadJson();

        await Assert.ThrowsAsync<InvalidWebhookSignatureException>(
            () => _sut.HandleAsync(body, signature!));
    }

    [Theory]
    [MemberData(nameof(Auth0WebhookServiceTestData.EmptySecrets), MemberType = typeof(Auth0WebhookServiceTestData))]
    public async Task HandleAsync_WebhookSecretIsNotConfigured_ThrowsInvalidWebhookSignatureException(string emptySecret)
    {
        const string irrelevantSecret = "irrelevant-secret";

        var settings = BuildSettings(emptySecret);
        var sut = new Auth0WebhookService(_userService.Object, Options.Create(settings));
        var body = BuildPayloadJson();
        var signature = ComputeValidSignature(body, irrelevantSecret);

        await Assert.ThrowsAsync<InvalidWebhookSignatureException>(
            () => sut.HandleAsync(body, signature));
    }

    [Fact]
    public async Task HandleAsync_SignatureDoesNotMatchComputedHash_ThrowsInvalidWebhookSignatureException()
    {
        const string invalidSignature = "sha256=0000000000000000000000000000000000000000000000000000000000000000";

        var body = BuildPayloadJson();

        await Assert.ThrowsAsync<InvalidWebhookSignatureException>(
            () => _sut.HandleAsync(body, invalidSignature));
    }

    [Fact]
    public async Task HandleAsync_PayloadUserIdIsMissing_ThrowsInvalidWebhookPayloadException()
    {
        var body = BuildPayloadJson(userId: null);
        var signature = ComputeValidSignature(body);

        await Assert.ThrowsAsync<InvalidWebhookPayloadException>(
            () => _sut.HandleAsync(body, signature));
    }

    [Fact]
    public async Task HandleAsync_IssDoesNotMatchConfiguredDomain_ThrowsInvalidWebhookPayloadException()
    {
        const string invalidIssuer = "https://wrong-tenant.example.com/";

        var body = BuildPayloadJson(iss: invalidIssuer);
        var signature = ComputeValidSignature(body);

        await Assert.ThrowsAsync<InvalidWebhookPayloadException>(
            () => _sut.HandleAsync(body, signature));
    }

    [Fact]
    public async Task HandleAsync_EventIsNotUserCreated_ThrowsUnsupportedWebhookEventException()
    {
        const string unsupportedEvent = "user.updated";

        var body = BuildPayloadJson(eventName: unsupportedEvent);
        var signature = ComputeValidSignature(body);

        await Assert.ThrowsAsync<UnsupportedWebhookEventException>(
            () => _sut.HandleAsync(body, signature));
    }

    [Fact]
    public async Task HandleAsync_UserAlreadyExists_ReturnsExistingUserWithoutCreating()
    {
        const int activeAccessStatus = 1;

        var existingUser = new GetUserDto
        {
            Id = Guid.NewGuid(),
            Nickname = DefaultNickname,
            CityId = null,
            IdentityId = DefaultIdentityId,
            AccessStatus = activeAccessStatus,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = null
        };
        var body = BuildPayloadJson(userId: DefaultIdentityId);
        var signature = ComputeValidSignature(body);

        _userService
            .Setup(x => x.GetUserByIdentityIdAsync(DefaultIdentityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var result = await _sut.HandleAsync(body, signature);

        Assert.Equal(existingUser, result);
        _userService.Verify(
            x => x.CreateUserAsync(It.IsAny<CreateUserDto>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_UserDoesNotExist_NicknameProvided_CreatesUserWithProvidedNickname()
    {
        var body = BuildPayloadJson(userId: DefaultIdentityId, nickname: ProvidedNickname, email: DefaultEmail);
        var signature = ComputeValidSignature(body);
        CreateUserDto? capturedDto = null;

        _userService
            .Setup(x => x.GetUserByIdentityIdAsync(DefaultIdentityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetUserDto?)null);
        _userService
            .Setup(x => x.CreateUserAsync(It.IsAny<CreateUserDto>(), It.IsAny<CancellationToken>()))
            .Callback<CreateUserDto, CancellationToken>((dto, _) => capturedDto = dto)
            .ReturnsAsync(BuildIrrelevantCreatedUserDto());

        await _sut.HandleAsync(body, signature);

        Assert.NotNull(capturedDto);
        Assert.Equal(DefaultIdentityId, capturedDto!.IdentityId);
        Assert.Equal(ProvidedNickname, capturedDto.Nickname);
        _userService.Verify(
            x => x.CreateUserAsync(It.IsAny<CreateUserDto>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_UserDoesNotExist_NicknameNullEmailProvided_UsesEmailLocalPartAsNickname()
    {
        var body = BuildPayloadJson(userId: DefaultIdentityId, nickname: null, email: DefaultEmail);
        var signature = ComputeValidSignature(body);
        CreateUserDto? capturedDto = null;

        _userService
            .Setup(x => x.GetUserByIdentityIdAsync(DefaultIdentityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetUserDto?)null);
        _userService
            .Setup(x => x.CreateUserAsync(It.IsAny<CreateUserDto>(), It.IsAny<CancellationToken>()))
            .Callback<CreateUserDto, CancellationToken>((dto, _) => capturedDto = dto)
            .ReturnsAsync(BuildIrrelevantCreatedUserDto());

        await _sut.HandleAsync(body, signature);

        Assert.NotNull(capturedDto);
        Assert.Equal(EmailLocalPart, capturedDto!.Nickname);
        _userService.Verify(
            x => x.CreateUserAsync(It.IsAny<CreateUserDto>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_UserDoesNotExist_NicknameIsWhitespaceOnly_FallsBackToEmail()
    {
        const string whitespaceNickname = "   ";

        var body = BuildPayloadJson(userId: DefaultIdentityId, nickname: whitespaceNickname, email: DefaultEmail);
        var signature = ComputeValidSignature(body);
        CreateUserDto? capturedDto = null;

        _userService
            .Setup(x => x.GetUserByIdentityIdAsync(DefaultIdentityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetUserDto?)null);
        _userService
            .Setup(x => x.CreateUserAsync(It.IsAny<CreateUserDto>(), It.IsAny<CancellationToken>()))
            .Callback<CreateUserDto, CancellationToken>((dto, _) => capturedDto = dto)
            .ReturnsAsync(BuildIrrelevantCreatedUserDto());

        await _sut.HandleAsync(body, signature);

        Assert.NotNull(capturedDto);
        Assert.Equal(EmailLocalPart, capturedDto!.Nickname);
        _userService.Verify(
            x => x.CreateUserAsync(It.IsAny<CreateUserDto>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_UserDoesNotExist_EmailStartsWithAtSymbol_CreatesUserWithEmptyNickname()
    {
        const string invalidEmail = "@example.com";

        var body = BuildPayloadJson(userId: DefaultIdentityId, nickname: null, email: invalidEmail);
        var signature = ComputeValidSignature(body);
        CreateUserDto? capturedDto = null;

        _userService
            .Setup(x => x.GetUserByIdentityIdAsync(DefaultIdentityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetUserDto?)null);
        _userService
            .Setup(x => x.CreateUserAsync(It.IsAny<CreateUserDto>(), It.IsAny<CancellationToken>()))
            .Callback<CreateUserDto, CancellationToken>((dto, _) => capturedDto = dto)
            .ReturnsAsync(BuildIrrelevantCreatedUserDto());

        await _sut.HandleAsync(body, signature);

        Assert.NotNull(capturedDto);
        Assert.Equal(string.Empty, capturedDto!.Nickname);
        _userService.Verify(
            x => x.CreateUserAsync(It.IsAny<CreateUserDto>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_UserDoesNotExist_NicknameAndEmailBothMissing_GeneratesFallbackNickname()
    {
        const string generatedNicknamePattern = "^User_[0-9a-f]{8}$";

        var body = BuildPayloadJson(userId: DefaultIdentityId, nickname: null, email: null);
        var signature = ComputeValidSignature(body);
        CreateUserDto? capturedDto = null;

        _userService
            .Setup(x => x.GetUserByIdentityIdAsync(DefaultIdentityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetUserDto?)null);
        _userService
            .Setup(x => x.CreateUserAsync(It.IsAny<CreateUserDto>(), It.IsAny<CancellationToken>()))
            .Callback<CreateUserDto, CancellationToken>((dto, _) => capturedDto = dto)
            .ReturnsAsync(BuildIrrelevantCreatedUserDto());

        await _sut.HandleAsync(body, signature);

        Assert.NotNull(capturedDto);
        Assert.Matches(generatedNicknamePattern, capturedDto!.Nickname);
        _userService.Verify(
            x => x.CreateUserAsync(It.IsAny<CreateUserDto>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
