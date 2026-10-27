using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Gateway.Tests.Infrastructure;
using Gateway.Tests.Infrastructure.Constants;
using Gateway.Tests.Integration.Routing.TestData;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Gateway.Tests.Integration.Routing.Tests;

public sealed class RoutingTests(GatewayWebAppFactory factory) : GatewayApiTestBase(factory)
{
    private const string ForwardedPath = "/api/v1/polls";
    private const string ForwardedQuery = "?page=2&size=5";
    private const string ForwardedBody = "{\"name\":\"gateway\"}";
    private const string BearerScheme = "Bearer";
    private const string Token = "test-token";
    private const string JsonContentType = "application/json";

    [Theory]
    [MemberData(nameof(RoutingTestData.FeedbackRoutes), MemberType = typeof(RoutingTestData))]
    [MemberData(nameof(RoutingTestData.UsersRoutes), MemberType = typeof(RoutingTestData))]
    [MemberData(nameof(RoutingTestData.PollsRoutes), MemberType = typeof(RoutingTestData))]
    public async Task Send_KnownRoute_ShouldReachExpectedService(string method, string path, string expectedService)
    {
        using var request = new HttpRequestMessage(new HttpMethod(method), path);

        var response = await Client.SendAsync(request);
        var echo = await ReadEchoAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expectedService, echo.Service);
        Assert.Equal(method, echo.Method);
        Assert.Equal(path, echo.Path);
    }

    [Theory]
    [MemberData(nameof(RoutingTestData.UnroutedPaths), MemberType = typeof(RoutingTestData))]
    public async Task Send_PathOutsideApi_ShouldReturnNotFound(string path)
    {
        var response = await Client.GetAsync(path);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Send_RequestWithQueryAndBody_ShouldForwardThemUnchanged()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, ForwardedPath + ForwardedQuery)
        {
            Content = new StringContent(ForwardedBody, Encoding.UTF8, JsonContentType)
        };

        var response = await Client.SendAsync(request);
        var echo = await ReadEchoAsync(response);

        Assert.Equal(ForwardedPath, echo.Path);
        Assert.Equal(ForwardedQuery, echo.Query);
        Assert.Equal(ForwardedBody, echo.Body);
    }

    [Fact]
    public async Task Send_RequestWithAuthorizationHeader_ShouldForwardItUnchanged()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, ForwardedPath);
        request.Headers.Authorization = new AuthenticationHeaderValue(BearerScheme, Token);

        var response = await Client.SendAsync(request);
        var echo = await ReadEchoAsync(response);

        Assert.Equal($"{BearerScheme} {Token}", echo.Authorization);
    }

    [Fact]
    public async Task Send_ServiceIsUnavailable_ShouldReturnBadGateway()
    {
        using var unavailableFactory = Factory.WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [TestConstants.ConfigurationKeys.ClusterAddress(TestConstants.Services.Feedback)] =
                        TestConstants.UnreachableAddress
                })));
        using var client = unavailableFactory.CreateClient();

        var response = await client.GetAsync("/api/v1/feedback");

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }
}
