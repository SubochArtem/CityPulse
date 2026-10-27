namespace Gateway.Tests.Infrastructure.Constants;

internal static class TestConstants
{
    public const string Environment = "IntegrationTesting";

    public static class Traits
    {
        public const string Category = "Category";
        public const string Integration = "Integration";
    }

    public static class ConfigurationKeys
    {
        public const string TokenLimit = "RateLimiting:TokenLimit";
    }
}
