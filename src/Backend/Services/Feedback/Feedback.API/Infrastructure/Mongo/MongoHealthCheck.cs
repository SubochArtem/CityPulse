using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Feedback.API.Infrastructure.Mongo;

public sealed class MongoHealthCheck(MongoDbContext context) : IHealthCheck
{
    private const string PingCommand = "ping";
    private const int PingValue = 1;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext healthCheckContext,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await context.FeedbackItems.Database.RunCommandAsync<BsonDocument>(
                new BsonDocument(PingCommand, PingValue),
                cancellationToken: cancellationToken);

            return HealthCheckResult.Healthy();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy(exception: exception);
        }
    }
}
