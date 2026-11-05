namespace Gateway.API.Constants;

public static class BffDefaults
{
    public const string CookieScheme = "BffCookie";
    public const string OidcScheme = "Auth0";
    public const string CookieName = "citypulse.session";
    public const string DisabledClientId = "bff-disabled";

    public const string LoginPath = "/bff/login";
    public const string UserPath = "/bff/user";
    public const string LogoutPath = "/bff/logout";
    public const string CallbackPath = "/bff/callback";

    public const string ReturnUrlParam = "returnUrl";
    public const string UiLocalesParam = "ui_locales";
    public const string AudienceParam = "audience";
    public const string SessionIdParam = "sid";
    public const string ClientIdParam = "client_id";
    public const string ReturnToParam = "returnTo";
    public const string ErrorParam = "error";
    public const string ErrorDescriptionParam = "error_description";
    public const string LoginFailedError = "login_failed";

    public const string RootPath = "/";
    public const string SessionIdClaim = "bff_sid";
    public const string SubjectClaim = "sub";
    public const string PermissionsClaim = "permissions";
    public const string AccessTokenName = "access_token";

    public const string OpenIdScope = "openid";
    public const string ProfileScope = "profile";
    public const string EmailScope = "email";
    public const string OfflineAccessScope = "offline_access";

    public const string CacheInstanceName = "citypulse:gateway:";
    public const string SessionKeyPrefix = "bff:session:";
    public const string DataProtectionApplicationName = "CityPulse.Gateway";
    public const string DataProtectionKeysKey = "citypulse:gateway:data-protection-keys";

    public static string Authority(string domain) => $"https://{domain}/";

    public static string LogoutEndpoint(string domain) => $"https://{domain}/v2/logout";
}
