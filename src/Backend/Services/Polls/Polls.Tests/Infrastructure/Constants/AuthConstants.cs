namespace Polls.Tests.Infrastructure.Constants;

internal static class AuthConstants
{
    public const string SchemeName = "TestScheme";
    public const string PermissionsClaimType = "permissions";

    public static class Headers
    {
        public const string Anonymous = "X-Test-Anonymous";
        public const string Permissions = "X-Test-Permissions";
        public const string UserId = "X-Test-User-Id";
        public const string CityId = "X-Test-City-Id";
        public const char PermissionsSeparator = ',';
    }
}
