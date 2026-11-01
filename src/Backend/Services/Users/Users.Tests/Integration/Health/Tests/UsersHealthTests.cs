using System.Net;
using System.Text.Json;
using CityPulse.Common.Api.Health;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Users.Tests.Infrastructure;
using Xunit;

namespace Users.Tests.Integration.Health.Tests;

public sealed class UsersHealthTests(IntegrationTestWebAppFactory factory) : UsersApiTestBase(factory)
{
    private const string HealthyStatus = "Healthy";
    private const string UnhealthyStatus = "Unhealthy";
    private const string PostgresCheck = "postgres";
    private const string MassTransitMarker = "masstransit";
    private const string RedisConnectionStringKey = "Redis:ConnectionString";
    private const string UnreachableRedis = "127.0.0.1:1";

    private HttpClient CreateClientWithUnreachableRedis() =>
        factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                [RedisConnectionStringKey] = UnreachableRedis
            }))).CreateClient();

    private HttpClient CreateClientWithInMemoryCache() =>
        factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IDistributedCache>();
            services.AddSingleton<IDistributedCache>(
                new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions())));
        })).CreateClient();

    private static async Task<(HttpStatusCode Status, JsonElement Body)> GetAsync(HttpClient client, string path)
    {
        var response = await client.GetAsync(path);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        return (response.StatusCode, document.RootElement.Clone());
    }

    [Fact]
    public async Task Live_ReturnsHealthyWithoutCheckingDependencies()
    {
        var (status, body) = await GetAsync(Client, HealthCheckConstants.LivePath);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal(HealthyStatus, body.GetProperty("status").GetString());
        Assert.Empty(body.GetProperty("checks").EnumerateObject());
    }

    [Fact]
    public async Task Ready_DependenciesReachable_ReturnsHealthyWithPostgresRedisAndBus()
    {
        using var client = CreateClientWithInMemoryCache();

        var (status, body) = await GetAsync(client, HealthCheckConstants.ReadyPath);

        var checks = body.GetProperty("checks").EnumerateObject().ToDictionary(p => p.Name, p => p.Value.GetString());
        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal(HealthyStatus, checks[PostgresCheck]);
        Assert.Equal(HealthyStatus, checks[HealthCheckConstants.RedisCheckName]);
        Assert.Contains(checks, check => check.Key.Contains(MassTransitMarker, StringComparison.OrdinalIgnoreCase)
                                         && check.Value == HealthyStatus);
    }

    [Fact]
    public async Task Ready_RedisUnreachable_ReturnsServiceUnavailableAndKeepsOtherChecksHealthy()
    {
        using var client = CreateClientWithUnreachableRedis();

        var (status, body) = await GetAsync(client, HealthCheckConstants.ReadyPath);

        var checks = body.GetProperty("checks");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, status);
        Assert.Equal(UnhealthyStatus, checks.GetProperty(HealthCheckConstants.RedisCheckName).GetString());
        Assert.Equal(HealthyStatus, checks.GetProperty(PostgresCheck).GetString());
    }

    [Fact]
    public async Task Live_RedisUnreachable_StillReturnsHealthy()
    {
        using var client = CreateClientWithUnreachableRedis();

        var (status, _) = await GetAsync(client, HealthCheckConstants.LivePath);

        Assert.Equal(HttpStatusCode.OK, status);
    }
}
