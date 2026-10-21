namespace Users.Tests.Infrastructure.Constants;

internal static class EnvironmentConstants
{
    public const string Environment = "IntegrationTesting";

    public static class Postgres
    {
        public const string Image = "postgres:16-alpine";
        public const string Database = "citypulse_users_test";
        public const string Username = "test_user";
        public const string Password = "test_password";
    }

    public static class RabbitMq
    {
        public const string Image = "rabbitmq:3-management";
        public const string Username = "guest";
        public const string Password = "guest";
        public const string VirtualHost = "/";
        public const int DefaultPort = 5672;
    }

    public static class Grpc
    {
        public const string DummyCitiesServiceUrl = "http://localhost:1";
    }

    public static class Auth0
    {
        public const string Domain = "test-tenant.example.com";
        public const string ClientId = "test-client-id";
        public const string ClientSecret = "test-client-secret";
        public const string Audience = "https://test-api";
        public const string ManagementApiClientId = "test-mgmt-client-id";
        public const string ManagementApiSecret = "test-mgmt-secret";
        public const string ManagementApiAudience = "https://test-tenant.example.com/api/v2/";
        public const string WebhookSecret = "test-webhook-secret";
    }
    
    public static class Traits
    {
        public const string Category = "Category";
        public const string Integration = "Integration";
    }

    public static class Respawner
    {
        public const string PublicSchema = "public";
        public const string EfMigrationsHistoryTable = "__EFMigrationsHistory";
    }
}
