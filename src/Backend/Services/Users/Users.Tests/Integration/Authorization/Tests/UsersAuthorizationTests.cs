using System.Net;
using System.Text;
using Users.Tests.Infrastructure;
using Users.Tests.Infrastructure.Constants;
using Users.Tests.Integration.Authorization.TestData;
using Xunit;

namespace Users.Tests.Integration.Authorization.Tests;

public sealed class UsersAuthorizationTests(IntegrationTestWebAppFactory factory) : UsersApiTestBase(factory)
{
    private const string PatchMethod = "PATCH";
    private const string JsonContentType = "application/json";
    private const string EmptyJsonBody = "{}";

    [Theory]
    [MemberData(nameof(UsersAuthorizationTestData.AllEndpoints), MemberType = typeof(UsersAuthorizationTestData))]
    public async Task Request_WithoutAuthentication_ReturnsUnauthorized(string method, string route)
    {
        using var client = CreateAnonymousClient();

        var response = await SendAsync(client, method, route, Guid.NewGuid());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(
        nameof(UsersAuthorizationTestData.EndpointsWithoutAccessPermissions),
        MemberType = typeof(UsersAuthorizationTestData))]
    public async Task Request_WithoutAccessPermissions_ReturnsForbidden(
        string method,
        string route,
        string permissions)
    {
        using var client = CreateClient(SplitPermissions(permissions));

        var response = await SendAsync(client, method, route, Guid.NewGuid());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [MemberData(
        nameof(UsersAuthorizationTestData.EndpointsWithAnyPermission),
        MemberType = typeof(UsersAuthorizationTestData))]
    public async Task Request_WithAnyPermission_ReturnsOk(string method, string route, string permission)
    {
        var user = await SeedUserAsync();
        using var client = CreateClient([permission]);

        var response = await SendAsync(client, method, route, user.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(UsersAuthorizationTestData.SelfEndpoints), MemberType = typeof(UsersAuthorizationTestData))]
    public async Task Request_WithSelfPermissionForOwnUser_ReturnsOk(
        string method,
        string route,
        string permission)
    {
        var user = await SeedUserAsync();
        using var client = CreateClient([permission], user.Id.ToString());

        var response = await SendAsync(client, method, route, user.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(UsersAuthorizationTestData.SelfEndpoints), MemberType = typeof(UsersAuthorizationTestData))]
    public async Task Request_WithSelfPermissionForAnotherUser_ReturnsForbidden(
        string method,
        string route,
        string permission)
    {
        var user = await SeedUserAsync();
        using var client = CreateClient([permission], Guid.NewGuid().ToString());

        var response = await SendAsync(client, method, route, user.Id);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [MemberData(
        nameof(UsersAuthorizationTestData.SelfEndpointsWithInvalidInternalUserId),
        MemberType = typeof(UsersAuthorizationTestData))]
    public async Task Request_WithSelfPermissionAndInvalidInternalUserId_ReturnsForbidden(
        string method,
        string route,
        string permission,
        string? internalUserId)
    {
        var user = await SeedUserAsync();
        using var client = CreateClient([permission], internalUserId);

        var response = await SendAsync(client, method, route, user.Id);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_WithReadMePermissionOnly_ReturnsForbidden()
    {
        var user = await SeedUserAsync();
        using var client = CreateClient([AuthConstants.Permissions.ReadMe], user.Id.ToString());

        var response = await client.GetAsync(UsersAuthorizationTestData.UsersRoute);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task HandleWebhook_WithoutAuthentication_IsNotRejectedByAuthorization()
    {
        using var client = CreateAnonymousClient();
        var body = Auth0WebhookRequestBuilder.BuildPayload();

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/Auth0Webhook")
        {
            Content = new StringContent(body, Encoding.UTF8, JsonContentType)
        };
        request.Headers.Add("X-Hub-Signature-256", Auth0WebhookRequestBuilder.Sign(body));

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static string[] SplitPermissions(string permissions) =>
        permissions.Split(AuthConstants.Headers.PermissionsSeparator, StringSplitOptions.RemoveEmptyEntries);

    private static Task<HttpResponseMessage> SendAsync(HttpClient client, string method, string route, Guid userId)
    {
        var request = new HttpRequestMessage(
            new HttpMethod(method),
            route.Replace(UsersAuthorizationTestData.UserIdPlaceholder, userId.ToString()));

        if (method == PatchMethod)
        {
            request.Content = new StringContent(EmptyJsonBody, Encoding.UTF8, JsonContentType);
        }

        return client.SendAsync(request);
    }
}
