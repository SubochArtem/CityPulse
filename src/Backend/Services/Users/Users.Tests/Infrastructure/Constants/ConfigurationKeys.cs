namespace Users.Tests.Infrastructure.Constants;

internal static class ConfigurationKeys
{
    public static class ConnectionStrings
    {
        public const string DefaultConnection = "ConnectionStrings:DefaultConnection";
    }

    public static class RabbitMq
    {
        public const string Host = "RabbitMq:Host";
        public const string Port = "RabbitMq:Port";
        public const string Username = "RabbitMq:Username";
        public const string Password = "RabbitMq:Password";
        public const string VirtualHost = "RabbitMq:VirtualHost";
    }

    public static class Grpc
    {
        public const string CitiesServiceUrl = "GrpcSettings:CitiesServiceUrl";
    }

    public static class Auth0
    {
        public const string Domain = "Auth0:Domain";
        public const string ClientId = "Auth0:ClientId";
        public const string ClientSecret = "Auth0:ClientSecret";
        public const string Audience = "Auth0:Audience";
        public const string ManagementApiClientId = "Auth0:ManagementApiClientId";
        public const string ManagementApiClientSecret = "Auth0:ManagementApiClientSecret";
        public const string ManagementApiAudience = "Auth0:ManagementApiAudience";
        public const string WebhookSecret = "Auth0:WebhookSecret";
    }
}
