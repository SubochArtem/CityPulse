using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Polls.Infrastructure.Persistence.Options;

namespace Polls.Infrastructure.Storage;

public sealed class MinioHealthCheck(
    IMinioClient minioClient,
    IOptions<ImageStorageOptions> options) : IHealthCheck
{
    private const string BucketMissingMessage = "The image storage bucket does not exist.";

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var args = new BucketExistsArgs().WithBucket(options.Value.BucketName);
            var exists = await minioClient.BucketExistsAsync(args, cancellationToken);

            return exists
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy(BucketMissingMessage);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy(exception: exception);
        }
    }
}
