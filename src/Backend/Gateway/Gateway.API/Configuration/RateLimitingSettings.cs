using System.ComponentModel.DataAnnotations;

namespace Gateway.API.Configuration;

public class RateLimitingSettings
{
    public const string SectionName = "RateLimiting";

    private const int DefaultTokenLimit = 100;
    private const int DefaultTokensPerPeriod = 10;
    private const int DefaultReplenishmentPeriodSeconds = 6;

    [Range(1, int.MaxValue)]
    public int TokenLimit { get; init; } = DefaultTokenLimit;

    [Range(1, int.MaxValue)]
    public int TokensPerPeriod { get; init; } = DefaultTokensPerPeriod;

    [Range(1, int.MaxValue)]
    public int ReplenishmentPeriodSeconds { get; init; } = DefaultReplenishmentPeriodSeconds;
}
