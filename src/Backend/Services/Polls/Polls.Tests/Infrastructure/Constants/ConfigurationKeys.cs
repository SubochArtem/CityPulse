namespace Polls.Tests.Infrastructure.Constants;

internal static class ConfigurationKeys
{
    public static class Database
    {
        public const string ConnectionString = "DatabaseSettings:ConnectionString";
    }

    public static class RabbitMq
    {
        public const string Host = "RabbitMq:Host";
        public const string Port = "RabbitMq:Port";
        public const string Username = "RabbitMq:Username";
        public const string Password = "RabbitMq:Password";
        public const string VirtualHost = "RabbitMq:VirtualHost";
    }

    public static class ImageStorage
    {
        public const string Endpoint = "ImageStorage:Endpoint";
        public const string AccessKey = "ImageStorage:AccessKey";
        public const string SecretKey = "ImageStorage:SecretKey";
        public const string BucketName = "ImageStorage:BucketName";
        public const string UseSsl = "ImageStorage:UseSsl";
    }

    public static class Auth0
    {
        public const string Domain = "Auth0:Domain";
        public const string Audience = "Auth0:Audience";
        public const string ClientId = "Auth0:ClientId";
    }
}
