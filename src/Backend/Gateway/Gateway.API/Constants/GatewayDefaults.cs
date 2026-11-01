namespace Gateway.API.Constants;

public static class GatewayDefaults
{
    public const string ReverseProxySectionName = "ReverseProxy";
    public const string HealthCheckPath = "/health";
    public const string ServicesHealthCheckPath = "/health/services";
    public const string ServicesHealthTag = "services";
    public const string ServiceReadyPath = "/health/ready";
    public const string ServiceHealthClientName = "ServiceHealth";
    public const string UnknownClientKey = "unknown";

    public static IReadOnlyList<string> ServiceClusters { get; } = ["users", "polls", "feedback"];
}
