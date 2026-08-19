using System.ComponentModel.DataAnnotations;

namespace Users.DataAccess.Settings;

public sealed class RabbitMqSettings
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
