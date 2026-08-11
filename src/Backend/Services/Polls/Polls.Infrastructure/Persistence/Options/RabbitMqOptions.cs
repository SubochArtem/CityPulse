using System.ComponentModel.DataAnnotations;

namespace Polls.Infrastructure.Persistence.Options;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    [Required]
    public string Host { get; init; } = string.Empty;
    [Range(1, ushort.MaxValue)]
    public ushort Port { get; init; } = 5672;
    [Required]
    public string Username { get; init; } = string.Empty;
    [Required]
    public string Password { get; init; } = string.Empty;
    [Required]
    public string VirtualHost { get; init; } = "/";
}
