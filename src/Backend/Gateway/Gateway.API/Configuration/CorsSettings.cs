using System.ComponentModel.DataAnnotations;

namespace Gateway.API.Configuration;

public class CorsSettings
{
    public const string SectionName = "Cors";

    [MinLength(1)]
    public string[] AllowedOrigins { get; init; } = [];
}
