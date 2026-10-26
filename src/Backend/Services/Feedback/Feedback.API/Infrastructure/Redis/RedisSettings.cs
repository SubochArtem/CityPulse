using System.ComponentModel.DataAnnotations;

namespace Feedback.API.Infrastructure.Redis;

public class RedisSettings
{
    public const string SectionName = "Redis";

    [Required]
    public required string ConnectionString { get; init; }

    [Required]
    public required string InstanceName { get; init; }

    [Range(1, 60000)]
    public int ConnectTimeoutMs { get; init; } = 500;

    [Range(1, 60000)]
    public int SyncTimeoutMs { get; init; } = 500;

    [Range(1, 60000)]
    public int AsyncTimeoutMs { get; init; } = 500;
}
