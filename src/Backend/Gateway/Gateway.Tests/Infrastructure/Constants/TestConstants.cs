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

    public static class Origins
    {
        public const string Allowed = "http://localhost:5173";
        public const string AnotherAllowed = "http://localhost:3000";
        public const string Foreign = "http://evil.example";
    }

    public static class Headers
    {
        public const string Origin = "Origin";
        public const string AccessControlRequestMethod = "Access-Control-Request-Method";
        public const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";
    }

    public static class Swagger
    {
        public const string ClientId = "swagger-test-client";
        public const string Audience = "https://swagger-test-api";
        public const string IndexPath = "/swagger/index.html";
        public const string RedirectPath = "/swagger/oauth2-redirect.html";
        public const string UpstreamDocumentPath = "/swagger/v1/swagger.json";
        public const string UsersTitle = "Users";
        public const string PollsTitle = "Polls";
        public const string FeedbackTitle = "Feedback";

        public static string DocumentPath(string service) => $"/swagger/{service}/swagger.json";
    }

    public static class ConfigurationKeys
    {
        public const string SwaggerEnabled = "Swagger:Enabled";
        public const string SwaggerClientId = "Swagger:ClientId";
        public const string SwaggerAudience = "Swagger:Audience";
        public const string TokenLimit = "RateLimiting:TokenLimit";
        public const string TokensPerPeriod = "RateLimiting:TokensPerPeriod";
        public const string ReplenishmentPeriodSeconds = "RateLimiting:ReplenishmentPeriodSeconds";
        public const string QueueLimit = "RateLimiting:QueueLimit";

        public static string ClusterAddress(string cluster) =>
            $"ReverseProxy:Clusters:{cluster}:Destinations:{cluster}:Address";
    }
}
