namespace Gateway.API.Configuration;

public class RedisSettings
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; init; } = string.Empty;
}
