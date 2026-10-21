using System.Net;
using System.Net.Http.Json;
using Users.Business.DTOs;
using Users.Tests.Infrastructure;
using Users.Tests.Integration.Webhook.TestData;
using Xunit;

namespace Users.Tests.Integration.Webhook.Tests;

public sealed class Auth0WebhookCreateUserTests(IntegrationTestWebAppFactory factory) : UsersApiTestBase(factory)
{
    [Fact]
    public async Task HandleWebhook_UserCreatedEvent_CreatesUser()
    {
        var body = Auth0WebhookRequestBuilder.BuildPayload();

        var response = await PostWebhookAsync(body, Auth0WebhookRequestBuilder.Sign(body));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await response.Content.ReadFromJsonAsync<GetUserDto>();
        Assert.NotNull(dto);
        Assert.Equal(Auth0WebhookRequestBuilder.DefaultIdentityId, dto.IdentityId);
        Assert.Equal(Auth0WebhookRequestBuilder.DefaultNickname, dto.Nickname);

        var created = await GetUserByIdentityIdFromDbAsync(Auth0WebhookRequestBuilder.DefaultIdentityId);
        Assert.NotNull(created);
        Assert.Equal(dto.Id, created.Id);
        Assert.Equal(Auth0WebhookRequestBuilder.DefaultNickname, created.Nickname);
    }

    [Theory]
    [MemberData(
        nameof(Auth0WebhookCreateUserTestData.NicknameSources),
        MemberType = typeof(Auth0WebhookCreateUserTestData))]
    public async Task HandleWebhook_UserCreatedEvent_UsesNicknameFromPayloadOrEmail(
        string? nickname,
        string? email,
        string expectedNickname)
    {
        var body = Auth0WebhookRequestBuilder.BuildPayload(nickname: nickname, email: email);

        var response = await PostWebhookAsync(body, Auth0WebhookRequestBuilder.Sign(body));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var created = await GetUserByIdentityIdFromDbAsync(Auth0WebhookRequestBuilder.DefaultIdentityId);
        Assert.NotNull(created);
        Assert.Equal(expectedNickname, created.Nickname);
    }

    [Theory]
    [MemberData(
        nameof(Auth0WebhookCreateUserTestData.GeneratedNicknameSources),
        MemberType = typeof(Auth0WebhookCreateUserTestData))]
    public async Task HandleWebhook_UserCreatedEventWithoutNicknameAndEmail_GeneratesNickname(
        string? nickname,
        string? email)
    {
        var body = Auth0WebhookRequestBuilder.BuildPayload(nickname: nickname, email: email);

        var response = await PostWebhookAsync(body, Auth0WebhookRequestBuilder.Sign(body));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var created = await GetUserByIdentityIdFromDbAsync(Auth0WebhookRequestBuilder.DefaultIdentityId);
        Assert.NotNull(created);
        Assert.StartsWith(Auth0WebhookCreateUserTestData.GeneratedNicknamePrefix, created.Nickname);
        Assert.Equal(Auth0WebhookCreateUserTestData.GeneratedNicknameLength, created.Nickname.Length);
    }

    [Fact]
    public async Task HandleWebhook_UserAlreadyExists_ReturnsExistingUserWithoutCreatingDuplicate()
    {
        var existing = await SeedUserAsync();
        var body = Auth0WebhookRequestBuilder.BuildPayload(
            userId: existing.IdentityId,
            nickname: Auth0WebhookCreateUserTestData.ProvidedNickname);

        var response = await PostWebhookAsync(body, Auth0WebhookRequestBuilder.Sign(body));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await response.Content.ReadFromJsonAsync<GetUserDto>();
        Assert.NotNull(dto);
        Assert.Equal(existing.Id, dto.Id);
        Assert.Equal(existing.Nickname, dto.Nickname);

        Assert.Equal(1, await CountUsersInDbAsync());
    }
}
