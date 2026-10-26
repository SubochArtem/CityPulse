using System.ComponentModel.DataAnnotations;

namespace Feedback.API.Infrastructure.Redis;

public class RedisSettings
{
    public const string SectionName = "Redis";

    [Required]
    public required string ConnectionString { get; init; }

    [Required]
    public required string InstanceName { get; init; }
}
