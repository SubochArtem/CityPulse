using System.ComponentModel.DataAnnotations;

namespace Polls.Infrastructure.Persistence.Options;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    [Required]
    public required string Host { get; init; }
    [Range(1, ushort.MaxValue)]
    public ushort Port { get; init; }
    [Required]
    public required string Username { get; init; }
    [Required]
    public required string Password { get; init; }
    [Required]
    public required string VirtualHost { get; init; }
}
