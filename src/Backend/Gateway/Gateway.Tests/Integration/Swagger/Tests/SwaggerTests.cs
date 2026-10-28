using System.Net;
using Gateway.Tests.Infrastructure;
using Gateway.Tests.Infrastructure.Constants;
using Gateway.Tests.Integration.Swagger.TestData;
using Xunit;

namespace Gateway.Tests.Integration.Swagger.Tests;

public sealed class SwaggerTests(GatewayWebAppFactory factory) : GatewayApiTestBase(factory)
{
    private const string ApiPath = "/api/v1/polls";

    private static readonly IReadOnlyDictionary<string, string?> SwaggerDisabled = new Dictionary<string, string?>
    {
        [TestConstants.ConfigurationKeys.SwaggerEnabled] = bool.FalseString
    };

    [Theory]
    [MemberData(nameof(SwaggerTestData.Services), MemberType = typeof(SwaggerTestData))]
    public async Task Get_Document_ShouldReachServiceWithUpstreamPath(string service)
    {
        var response = await Client.GetAsync(TestConstants.Swagger.DocumentPath(service));
        var echo = await ReadEchoAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(service, echo.Service);
        Assert.Equal(TestConstants.Swagger.UpstreamDocumentPath, echo.Path);
    }

    [Fact]
    public async Task Get_SwaggerRoot_ShouldServeUi()
    {
        var response = await Client.GetAsync("/swagger");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(TestConstants.Swagger.DocumentPath(TestConstants.Services.Polls), html);
    }

    [Fact]
    public async Task Get_Index_ShouldListAllDocuments()
    {
        var response = await Client.GetAsync(TestConstants.Swagger.IndexPath);
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        foreach (var service in new[] { TestConstants.Services.Users, TestConstants.Services.Polls, TestConstants.Services.Feedback })
        {
            Assert.Contains(TestConstants.Swagger.DocumentPath(service), html);
        }
    }

    [Theory]
    [MemberData(nameof(SwaggerTestData.DocumentTitles), MemberType = typeof(SwaggerTestData))]
    public async Task Get_Index_ShouldContainDocumentTitle(string title)
    {
        var response = await Client.GetAsync(TestConstants.Swagger.IndexPath);
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains(title, html);
    }

    [Fact]
    public async Task Get_Index_ShouldConfigureOAuthWithClientIdAndAudience()
    {
        var response = await Client.GetAsync(TestConstants.Swagger.IndexPath);
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains(TestConstants.Swagger.ClientId, html);
        Assert.Contains(TestConstants.Swagger.Audience, html);
    }

    [Fact]
    public async Task Get_OAuthRedirectPage_ShouldBeServed()
    {
        var response = await Client.GetAsync(TestConstants.Swagger.RedirectPath);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_Index_SwaggerDisabled_ShouldReturnNotFound()
    {
        using var client = CreateClient(SwaggerDisabled);

        var response = await client.GetAsync(TestConstants.Swagger.IndexPath);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(SwaggerTestData.Services), MemberType = typeof(SwaggerTestData))]
    public async Task Get_Document_SwaggerDisabled_ShouldReturnNotFound(string service)
    {
        using var client = CreateClient(SwaggerDisabled);

        var response = await client.GetAsync(TestConstants.Swagger.DocumentPath(service));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_Api_SwaggerDisabled_ShouldStillBeProxied()
    {
        using var client = CreateClient(SwaggerDisabled);

        var response = await client.GetAsync(ApiPath);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
