namespace CityPulse.Common.Api.Health;

public static class HealthCheckConstants
{
    public const string LivePath = "/health/live";
    public const string ReadyPath = "/health/ready";
    public const string ReadyTag = "ready";
    public const string RedisCheckName = "redis";
}
