using System.Net;
using System.Text.Json;
using CityPulse.Common.Api.Extensions;
using CityPulse.Common.Api.Health;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CityPulse.Common.Api.Tests.Health;

public sealed class HealthCheckTests
{
    private const string LoopbackUrl = "http://127.0.0.1:0";
    private const string DatabaseCheck = "database";
    private const string UntaggedCheck = "untagged";
    private const string SecretDetail = "connection string with password";
    private const string HealthyStatus = "Healthy";
    private const string UnhealthyStatus = "Unhealthy";
    private const string JsonContentType = "application/json";

    private static async Task<WebApplication> StartAsync(
        Action<IServiceCollection, IHealthChecksBuilder> configure)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls(LoopbackUrl);
        builder.Logging.ClearProviders();
        configure(builder.Services, builder.Services.AddHealthChecks());

        var app = builder.Build();
        app.MapCityPulseHealthChecks();
        await app.StartAsync();

        return app;
    }

    private static HttpClient CreateClient(WebApplication app) =>
        new() { BaseAddress = new Uri(app.Urls.First()) };

    private static async Task<(HttpStatusCode Status, string? ContentType, JsonElement Body)> GetAsync(
        WebApplication app,
        string path)
    {
        using var client = CreateClient(app);
        var response = await client.GetAsync(path);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        return (response.StatusCode, response.Content.Headers.ContentType?.MediaType, document.RootElement.Clone());
    }

    private static void AddReadyCheck(IHealthChecksBuilder checks, string name, HealthCheckResult result) =>
        checks.AddCheck(name, () => result, tags: [HealthCheckConstants.ReadyTag]);

    [Fact]
    public async Task Live_FailingReadyCheck_StillReturnsHealthy()
    {
        await using var app = await StartAsync((_, checks) =>
            AddReadyCheck(checks, DatabaseCheck, HealthCheckResult.Unhealthy()));

        var (status, _, body) = await GetAsync(app, HealthCheckConstants.LivePath);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal(HealthyStatus, body.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Live_Response_IsJsonWithoutAnyChecks()
    {
        await using var app = await StartAsync((_, checks) =>
            AddReadyCheck(checks, DatabaseCheck, HealthCheckResult.Healthy()));

        var (_, contentType, body) = await GetAsync(app, HealthCheckConstants.LivePath);

        Assert.Equal(JsonContentType, contentType);
        Assert.Empty(body.GetProperty("checks").EnumerateObject());
    }

    [Fact]
    public async Task Ready_AllChecksHealthy_ReturnsOkWithCheckStatuses()
    {
        await using var app = await StartAsync((_, checks) =>
            AddReadyCheck(checks, DatabaseCheck, HealthCheckResult.Healthy()));

        var (status, _, body) = await GetAsync(app, HealthCheckConstants.ReadyPath);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal(HealthyStatus, body.GetProperty("status").GetString());
        Assert.Equal(HealthyStatus, body.GetProperty("checks").GetProperty(DatabaseCheck).GetString());
    }

    [Fact]
    public async Task Ready_UnhealthyCheck_ReturnsServiceUnavailableWithoutExceptionDetails()
    {
        await using var app = await StartAsync((_, checks) =>
            AddReadyCheck(checks, DatabaseCheck, HealthCheckResult.Unhealthy(exception: new InvalidOperationException(SecretDetail))));
        using var client = CreateClient(app);

        var response = await client.GetAsync(HealthCheckConstants.ReadyPath);
        var text = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Contains(UnhealthyStatus, text);
        Assert.Contains(DatabaseCheck, text);
        Assert.DoesNotContain(SecretDetail, text);
    }

    [Fact]
    public async Task Ready_CheckWithoutReadyTag_IsNotExecuted()
    {
        var executions = 0;
        await using var app = await StartAsync((_, checks) =>
        {
            AddReadyCheck(checks, DatabaseCheck, HealthCheckResult.Healthy());
            checks.AddCheck(UntaggedCheck, () =>
            {
                executions++;

                return HealthCheckResult.Unhealthy();
            });
        });

        var (status, _, body) = await GetAsync(app, HealthCheckConstants.ReadyPath);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal(0, executions);
        Assert.False(body.GetProperty("checks").TryGetProperty(UntaggedCheck, out _));
    }

    [Fact]
    public async Task Ready_NoChecksRegistered_ReturnsHealthy()
    {
        await using var app = await StartAsync((_, _) => { });

        var (status, _, body) = await GetAsync(app, HealthCheckConstants.ReadyPath);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal(HealthyStatus, body.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Ready_RedisReachable_ReportsRedisHealthy()
    {
        var cache = new Mock<IDistributedCache>();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);
        await using var app = await StartAsync((services, checks) =>
        {
            services.AddSingleton(cache.Object);
            checks.AddCityPulseRedisCheck();
        });

        var (status, _, body) = await GetAsync(app, HealthCheckConstants.ReadyPath);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal(HealthyStatus, body.GetProperty("checks").GetProperty(HealthCheckConstants.RedisCheckName).GetString());
    }

    [Fact]
    public async Task Ready_RedisThrows_ReportsRedisUnhealthyWithServiceUnavailable()
    {
        var cache = new Mock<IDistributedCache>();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(SecretDetail));
        await using var app = await StartAsync((services, checks) =>
        {
            services.AddSingleton(cache.Object);
            checks.AddCityPulseRedisCheck();
        });

        var (status, _, body) = await GetAsync(app, HealthCheckConstants.ReadyPath);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, status);
        Assert.Equal(UnhealthyStatus, body.GetProperty("checks").GetProperty(HealthCheckConstants.RedisCheckName).GetString());
    }
}
