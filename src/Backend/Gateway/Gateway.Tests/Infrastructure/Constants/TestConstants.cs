namespace Gateway.Tests.Infrastructure.Constants;

internal static class TestConstants
{
    public const string Environment = "IntegrationTesting";
    public const string LoopbackUrl = "http://127.0.0.1:0";
    public const string UnreachableAddress = "http://127.0.0.1:1";

    public static class Traits
    {
        public const string Category = "Category";
        public const string Integration = "Integration";
    }

    public static class Services
    {
        public const string Users = "users";
        public const string Polls = "polls";
        public const string Feedback = "feedback";
    }

    public static class ConfigurationKeys
    {
        public const string TokenLimit = "RateLimiting:TokenLimit";

        public static string ClusterAddress(string cluster) =>
            $"ReverseProxy:Clusters:{cluster}:Destinations:{cluster}:Address";
    }
}
