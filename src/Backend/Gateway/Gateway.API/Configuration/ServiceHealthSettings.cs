using System.ComponentModel.DataAnnotations;

namespace Gateway.API.Configuration;

public class ServiceHealthSettings
{
    public const string SectionName = "ServiceHealth";

    private const int DefaultTimeoutSeconds = 4;

    [Range(1, 60)]
    public int TimeoutSeconds { get; init; } = DefaultTimeoutSeconds;
}
