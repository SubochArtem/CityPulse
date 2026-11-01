using System.Net;
using System.Text.Json;
using CityPulse.Common.Api.Health;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Minio;
using Minio.DataModel.Args;
using Moq;
using Polls.Tests.Infrastructure;
using Xunit;

namespace Polls.Tests.Integration.Health.Tests;

public sealed class PollsHealthTests(PollsWebAppFactory factory) : PollsApiTestBase(factory)
{
    private const string HealthyStatus = "Healthy";
    private const string UnhealthyStatus = "Unhealthy";
    private const string PostgresCheck = "postgres";
    private const string MinioCheck = "minio";
    private const string SecretDetail = "storage credentials rejected";

    private HttpClient CreateClientWithMinio(Action<Mock<IMinioClient>> setup)
    {
        var minio = new Mock<IMinioClient>();
        setup(minio);

        return Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IMinioClient>();
            services.AddSingleton(minio.Object);
        })).CreateClient();
    }

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
    public async Task Live_ReturnsHealthyWithoutCheckingDependencies()
    {
        using var client = Factory.CreateClient();

        var (status, _, body) = await GetAsync(client, HealthCheckConstants.LivePath);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal(HealthyStatus, body.GetProperty("status").GetString());
        Assert.Empty(body.GetProperty("checks").EnumerateObject());
    }

    [Fact]
    public async Task Ready_PostgresAndBucketAvailable_ReturnsHealthy()
    {
        using var client = CreateClientWithMinio(minio => minio
            .Setup(m => m.BucketExistsAsync(It.IsAny<BucketExistsArgs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true));

        var (status, _, body) = await GetAsync(client, HealthCheckConstants.ReadyPath);

        var checks = body.GetProperty("checks");
        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal(HealthyStatus, checks.GetProperty(PostgresCheck).GetString());
        Assert.Equal(HealthyStatus, checks.GetProperty(MinioCheck).GetString());
    }

    [Fact]
    public async Task Ready_BucketMissing_ReturnsServiceUnavailableAndKeepsPostgresHealthy()
    {
        using var client = CreateClientWithMinio(minio => minio
            .Setup(m => m.BucketExistsAsync(It.IsAny<BucketExistsArgs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false));

        var (status, _, body) = await GetAsync(client, HealthCheckConstants.ReadyPath);

        var checks = body.GetProperty("checks");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, status);
        Assert.Equal(UnhealthyStatus, checks.GetProperty(MinioCheck).GetString());
        Assert.Equal(HealthyStatus, checks.GetProperty(PostgresCheck).GetString());
    }

    [Fact]
    public async Task Ready_MinioThrows_ReturnsServiceUnavailableWithoutExceptionDetails()
    {
        using var client = CreateClientWithMinio(minio => minio
            .Setup(m => m.BucketExistsAsync(It.IsAny<BucketExistsArgs>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(SecretDetail)));

        var (status, text, body) = await GetAsync(client, HealthCheckConstants.ReadyPath);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, status);
        Assert.Equal(UnhealthyStatus, body.GetProperty("checks").GetProperty(MinioCheck).GetString());
        Assert.DoesNotContain(SecretDetail, text);
    }
}
