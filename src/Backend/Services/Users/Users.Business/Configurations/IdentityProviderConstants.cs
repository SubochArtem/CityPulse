namespace Users.Business.Configurations;

public static class IdentityProviderConstants
{
    public const string Auth0HttpClientName = "Auth0Client";

    public const string Auth0ApiV2Path = "/api/v2";

    public const int TokenExpiryBufferSeconds = 300;

    public const string Auth0ConfigurationSection = "Auth0";

    public const string WebhookUserCreatedEvent = "user.created";

    public const string WebhookSignatureHeader = "X-Hub-Signature-256";

    public const string WebhookSignaturePrefix = "sha256=";
}
