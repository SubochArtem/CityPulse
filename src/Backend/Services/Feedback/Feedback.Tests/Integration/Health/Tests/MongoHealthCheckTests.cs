using Feedback.API.Infrastructure.Mongo;
using Feedback.Tests.Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Xunit;

namespace Feedback.Tests.Integration.Health.Tests;

public sealed class MongoHealthCheckTests(MongoFixture fixture) : MongoTestsBase(fixture)
{
    private const string UnreachableConnectionString = "mongodb://127.0.0.1:1/?serverSelectionTimeoutMS=500";
    private const string DatabaseName = "health_unreachable";
    private const string CollectionName = "feedback";

    private static MongoDbContext CreateUnreachableContext() => new(Options.Create(new MongoDbSettings
    {
        ConnectionString = UnreachableConnectionString,
        DatabaseName = DatabaseName,
        FeedbackCollectionName = CollectionName
    }));

    [Fact]
    public async Task CheckHealthAsync_MongoReachable_ReturnsHealthy()
    {
        var check = new MongoHealthCheck(Context);

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_MongoUnreachable_ReturnsUnhealthyWithException()
    {
        var check = new MongoHealthCheck(CreateUnreachableContext());

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.NotNull(result.Exception);
    }

    [Fact]
    public async Task CheckHealthAsync_AlreadyCancelled_ThrowsOperationCanceled()
    {
        var check = new MongoHealthCheck(Context);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => check.CheckHealthAsync(new HealthCheckContext(), cancellation.Token));
    }
}
