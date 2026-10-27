using System.ComponentModel.DataAnnotations;

namespace CityPulse.Common.Api.Caching;

public class RedisSettings
{
    public const string SectionName = "Redis";

    [Required]
    public required string ConnectionString { get; init; }

    [Required]
    public required string InstanceName { get; init; }

    [Range(1, 60000)]
    public required int ConnectTimeoutMs { get; init; }

    [Range(1, 60000)]
    public required int SyncTimeoutMs { get; init; }

    [Range(1, 60000)]
    public required int AsyncTimeoutMs { get; init; }
}
