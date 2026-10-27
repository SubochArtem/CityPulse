using System.Net;
using Gateway.Tests.Infrastructure;
using Gateway.Tests.Infrastructure.Constants;
using Gateway.Tests.Integration.Cors.TestData;
using Xunit;

namespace Gateway.Tests.Integration.Cors.Tests;

public sealed class CorsTests(GatewayWebAppFactory factory) : GatewayApiTestBase(factory)
{
    private const string Path = "/api/v1/polls";
    private const string GetMethod = "GET";

    [Theory]
    [MemberData(nameof(CorsTestData.AllowedOrigins), MemberType = typeof(CorsTestData))]
    public async Task Preflight_AllowedOrigin_ShouldReturnAllowOriginHeader(string origin)
    {
        using var request = CreateRequest(HttpMethod.Options, Path, origin);
        request.Headers.Add(TestConstants.Headers.AccessControlRequestMethod, GetMethod);

        var response = await Client.SendAsync(request);

        Assert.Equal(origin, GetAllowOrigin(response));
    }

    [Fact]
    public async Task Preflight_ForeignOrigin_ShouldNotReturnAllowOriginHeader()
    {
        using var request = CreateRequest(HttpMethod.Options, Path, TestConstants.Origins.Foreign);
        request.Headers.Add(TestConstants.Headers.AccessControlRequestMethod, GetMethod);

        var response = await Client.SendAsync(request);

        Assert.Null(GetAllowOrigin(response));
    }

    [Theory]
    [MemberData(nameof(CorsTestData.AllowedOrigins), MemberType = typeof(CorsTestData))]
    public async Task Send_AllowedOrigin_ShouldReturnAllowOriginHeader(string origin)
    {
        using var request = CreateRequest(HttpMethod.Get, Path, origin);

        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(origin, GetAllowOrigin(response));
    }

    [Fact]
    public async Task Send_ForeignOrigin_ShouldNotReturnAllowOriginHeader()
    {
        using var request = CreateRequest(HttpMethod.Get, Path, TestConstants.Origins.Foreign);

        var response = await Client.SendAsync(request);

        Assert.Null(GetAllowOrigin(response));
    }
}
