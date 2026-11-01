using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Gateway.Tests.Infrastructure;
using Gateway.Tests.Infrastructure.Constants;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Gateway.Tests.Integration.Health.Tests;

public sealed class ServicesHealthTests(GatewayWebAppFactory factory) : GatewayApiTestBase(factory)
{
    private const string HealthyStatus = "Healthy";
    private const string DegradedStatus = "Degraded";
    private const string UnhealthyStatus = "Unhealthy";
    private const string DegradedBody = "{\"status\":\"Degraded\",\"checks\":{}}";
    private const int SlowResponseSeconds = 5;
    private const int TimeoutSeconds = 1;
    private const string UnreachableMarker = "127.0.0.1";

    private static readonly string[] AllServices =
        [TestConstants.Services.Users, TestConstants.Services.Polls, TestConstants.Services.Feedback];

    private static Dictionary<string, string?> Address(string service, string address) =>
        new() { [TestConstants.ConfigurationKeys.ClusterAddress(service)] = address };

    private static async Task<(HttpStatusCode Status, string Text, JsonElement Body)> GetAsync(
        HttpClient client,
        string path)
    {
        var response = await client.GetAsync(path);
        var text = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(text);

        return (response.StatusCode, text, document.RootElement.Clone());
    }

    [Fact]
    public async Task Get_ServicesHealth_AllServicesReady_ReturnsHealthyForEach()
    {
        var (status, _, body) = await GetAsync(Client, TestConstants.HealthPaths.Services);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal(HealthyStatus, body.GetProperty("status").GetString());
        foreach (var service in AllServices)
        {
            Assert.Equal(HealthyStatus, body.GetProperty("services").GetProperty(service).GetString());
        }
    }

    [Fact]
    public async Task Get_ServicesHealth_ServiceUnreachable_ReturnsServiceUnavailableAndKeepsOthersHealthy()
    {
        using var client = CreateClient(Address(TestConstants.Services.Feedback, TestConstants.UnreachableAddress));

        var (status, _, body) = await GetAsync(client, TestConstants.HealthPaths.Services);

        var services = body.GetProperty("services");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, status);
        Assert.Equal(UnhealthyStatus, services.GetProperty(TestConstants.Services.Feedback).GetString());
        Assert.Equal(HealthyStatus, services.GetProperty(TestConstants.Services.Users).GetString());
        Assert.Equal(HealthyStatus, services.GetProperty(TestConstants.Services.Polls).GetString());
    }

    [Fact]
    public async Task Get_ServicesHealth_ServiceReportsDegraded_ReturnsOkWithDegradedStatus()
    {
        await using var stub = await StubBackend.StartAsync(
            TestConstants.Services.Polls,
            context => context.Response.WriteAsync(DegradedBody));
        using var client = CreateClient(Address(TestConstants.Services.Polls, stub.Address));

        var (status, _, body) = await GetAsync(client, TestConstants.HealthPaths.Services);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal(DegradedStatus, body.GetProperty("status").GetString());
        Assert.Equal(DegradedStatus, body.GetProperty("services").GetProperty(TestConstants.Services.Polls).GetString());
    }

    [Fact]
    public async Task Get_ServicesHealth_ServiceReturnsServiceUnavailable_ReportsItUnhealthy()
    {
        await using var stub = await StubBackend.StartAsync(
            TestConstants.Services.Users,
            context =>
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;

                return Task.CompletedTask;
            });
        using var client = CreateClient(Address(TestConstants.Services.Users, stub.Address));

        var (status, _, body) = await GetAsync(client, TestConstants.HealthPaths.Services);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, status);
        Assert.Equal(UnhealthyStatus, body.GetProperty("services").GetProperty(TestConstants.Services.Users).GetString());
    }

    [Fact]
    public async Task Get_ServicesHealth_AsksEachServiceForItsReadyEndpoint()
    {
        var paths = new List<string>();
        await using var stub = await StubBackend.StartAsync(
            TestConstants.Services.Users,
            context =>
            {
                lock (paths)
                {
                    paths.Add(context.Request.Path.Value ?? string.Empty);
                }

                return context.Response.WriteAsync(string.Empty);
            });
        var settings = AllServices.ToDictionary(
            service => TestConstants.ConfigurationKeys.ClusterAddress(service),
            _ => (string?)stub.Address);
        using var client = CreateClient(settings);

        await GetAsync(client, TestConstants.HealthPaths.Services);

        Assert.Equal(AllServices.Length, paths.Count);
        Assert.All(paths, path => Assert.Equal(TestConstants.HealthPaths.ServiceReady, path));
    }

    [Fact]
    public async Task Get_Health_DoesNotCallAnyService()
    {
        var requests = 0;
        await using var stub = await StubBackend.StartAsync(
            TestConstants.Services.Users,
            context =>
            {
                Interlocked.Increment(ref requests);

                return context.Response.WriteAsync(string.Empty);
            });
        var settings = AllServices.ToDictionary(
            service => TestConstants.ConfigurationKeys.ClusterAddress(service),
            _ => (string?)stub.Address);
        using var client = CreateClient(settings);

        var response = await client.GetAsync(TestConstants.HealthPaths.Live);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, requests);
    }

    [Fact]
    public async Task Get_ServicesHealth_ServiceUnreachable_DoesNotLeakAddressOrErrorDetails()
    {
        using var client = CreateClient(Address(TestConstants.Services.Polls, TestConstants.UnreachableAddress));

        var (_, text, _) = await GetAsync(client, TestConstants.HealthPaths.Services);

        Assert.DoesNotContain(UnreachableMarker, text);
    }

    [Fact]
    public async Task Get_ServicesHealth_SlowService_TimesOutAsUnhealthy()
    {
        await using var stub = await StubBackend.StartAsync(
            TestConstants.Services.Feedback,
            async context =>
            {
                await Task.Delay(TimeSpan.FromSeconds(SlowResponseSeconds));
                await context.Response.WriteAsync(string.Empty);
            });
        var settings = Address(TestConstants.Services.Feedback, stub.Address);
        settings[TestConstants.ConfigurationKeys.ServiceHealthTimeoutSeconds] = TimeoutSeconds.ToString();
        using var client = CreateClient(settings);
        var stopwatch = Stopwatch.StartNew();

        var (status, _, body) = await GetAsync(client, TestConstants.HealthPaths.Services);

        stopwatch.Stop();
        Assert.Equal(HttpStatusCode.ServiceUnavailable, status);
        Assert.Equal(UnhealthyStatus, body.GetProperty("services").GetProperty(TestConstants.Services.Feedback).GetString());
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(SlowResponseSeconds));
    }
}
