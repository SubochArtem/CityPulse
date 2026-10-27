using System.Net;
using Gateway.Tests.Infrastructure;
using Gateway.Tests.Infrastructure.Constants;
using Xunit;

namespace Gateway.Tests.Integration.Health.Tests;

public sealed class HealthCheckTests(GatewayWebAppFactory factory) : GatewayApiTestBase(factory)
{
    private const string HealthPath = "/health";
    private const string HealthyStatus = "Healthy";

    [Fact]
    public async Task Get_Health_ShouldReturnHealthy()
    {
        var response = await Client.GetAsync(HealthPath);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(HealthyStatus, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Get_Health_ServicesAreUnavailable_ShouldStillReturnHealthy()
    {
        using var client = CreateClient(new Dictionary<string, string?>
        {
            [TestConstants.ConfigurationKeys.ClusterAddress(TestConstants.Services.Users)] = TestConstants.UnreachableAddress,
            [TestConstants.ConfigurationKeys.ClusterAddress(TestConstants.Services.Polls)] = TestConstants.UnreachableAddress,
            [TestConstants.ConfigurationKeys.ClusterAddress(TestConstants.Services.Feedback)] = TestConstants.UnreachableAddress
        });

        var response = await client.GetAsync(HealthPath);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
