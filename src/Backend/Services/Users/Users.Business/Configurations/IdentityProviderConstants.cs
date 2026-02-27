namespace Users.Business.Configurations;

public static class IdentityProviderConstants
{
    public const string Auth0HttpClientName = "Auth0Client";

    public const string Auth0ApiV2Path = "/api/v2";

    public const int TokenExpiryBufferSeconds = 300;

    public const string Auth0ConfigurationSection = "Auth0";
}
