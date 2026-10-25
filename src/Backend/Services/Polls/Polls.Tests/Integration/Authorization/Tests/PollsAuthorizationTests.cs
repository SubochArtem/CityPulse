using System.Net;
using Polls.Tests.Infrastructure;
using Polls.Tests.Integration.Authorization.TestData;
using Xunit;

namespace Polls.Tests.Integration.Authorization.Tests;

public sealed class PollsAuthorizationTests(PollsWebAppFactory factory) : PollsApiTestBase(factory)
{
    private const int ServerErrorThreshold = 500;

    private static HttpRequestMessage BuildRequest(EndpointCase endpoint) => new(endpoint.Method, endpoint.Route);

    private static void AssertPassedAuthorization(HttpStatusCode status)
    {
        Assert.NotEqual(HttpStatusCode.Unauthorized, status);
        Assert.NotEqual(HttpStatusCode.Forbidden, status);
        Assert.True((int)status < ServerErrorThreshold, $"Unexpected server error: {(int)status}");
    }

    [Theory]
    [MemberData(nameof(PollsAuthorizationTestData.PublicEndpoints), MemberType = typeof(PollsAuthorizationTestData))]
    public async Task PublicEndpoint_AnonymousUser_IsNotRejectedByAuthorization(EndpointCase endpoint)
    {
        var response = await CreateAnonymousClient().SendAsync(BuildRequest(endpoint));

        AssertPassedAuthorization(response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(PollsAuthorizationTestData.ProtectedEndpoints), MemberType = typeof(PollsAuthorizationTestData))]
    public async Task ProtectedEndpoint_AnonymousUser_ReturnsUnauthorized(EndpointCase endpoint)
    {
        var response = await CreateAnonymousClient().SendAsync(BuildRequest(endpoint));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(PollsAuthorizationTestData.ProtectedEndpoints), MemberType = typeof(PollsAuthorizationTestData))]
    public async Task ProtectedEndpoint_UserWithoutPermissions_ReturnsForbidden(EndpointCase endpoint)
    {
        var response = await CreateClient([]).SendAsync(BuildRequest(endpoint));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(PollsAuthorizationTestData.ProtectedEndpoints), MemberType = typeof(PollsAuthorizationTestData))]
    public async Task ProtectedEndpoint_UserWithUnrelatedPermission_ReturnsForbidden(EndpointCase endpoint)
    {
        var response = await CreateClient([PollsAuthorizationTestData.Unrelated]).SendAsync(BuildRequest(endpoint));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(PollsAuthorizationTestData.ProtectedEndpoints), MemberType = typeof(PollsAuthorizationTestData))]
    public async Task ProtectedEndpoint_UserWithRequiredPermission_IsNotRejectedByAuthorization(EndpointCase endpoint)
    {
        var response = await CreateClient([endpoint.Policy]).SendAsync(BuildRequest(endpoint));

        AssertPassedAuthorization(response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(PollsAuthorizationTestData.CrossOverEndpoints), MemberType = typeof(PollsAuthorizationTestData))]
    public async Task ProtectedEndpoint_UserWithPermissionOfAnotherScope_ReturnsForbidden(EndpointCase endpoint)
    {
        var response = await CreateClient([endpoint.Policy]).SendAsync(BuildRequest(endpoint));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
