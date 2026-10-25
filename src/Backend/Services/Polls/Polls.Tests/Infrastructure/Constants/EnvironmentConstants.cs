namespace Polls.Tests.Infrastructure.Constants;

internal static class EnvironmentConstants
{
    public const string Environment = "IntegrationTesting";

    public static class Postgres
    {
        public const string Image = "postgres:16-alpine";
        public const string Database = "citypulse_polls_test";
        public const string Username = "test_user";
        public const string Password = "test_password";
    }

    public static class RabbitMq
    {
        public const string Host = "localhost";
        public const string Port = "5672";
        public const string Username = "guest";
        public const string Password = "guest";
        public const string VirtualHost = "/";
    }

    public static class ImageStorage
    {
        public const string Endpoint = "localhost:9000";
        public const string AccessKey = "test-access-key";
        public const string SecretKey = "test-secret-key";
        public const string BucketName = "polls-images-test";
        public const string UseSsl = "false";
        public const string UrlPrefix = "https://storage.example.com/";
        public const string UploadedPrefix = "uploaded-";
    }

    public static class Auth0
    {
        public const string Domain = "test-tenant.example.com";
        public const string Audience = "https://test-api";
        public const string ClientId = "test-client-id";
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
