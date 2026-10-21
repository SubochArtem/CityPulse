namespace Users.Tests.Infrastructure.Constants;

internal static class AuthConstants
{
    public const string SchemeName = "Test";
    public const string PermissionsClaimType = "permissions";

    public static class Headers
    {
        public const string Permissions = "X-Test-Permissions";
        public const string InternalUserId = "X-Test-Internal-User-Id";
        public const string Anonymous = "X-Test-Anonymous";
        public const char PermissionsSeparator = ',';
    }

    public static class Permissions
    {
        public const string ReadMe = "users.read.me";
        public const string ReadAny = "users.read.any";
        public const string UpdateMe = "users.update.me";
        public const string UpdateAny = "users.update.any";
        public const string DeleteAny = "users.delete.any";
        public const string ActivateAny = "users.activate.any";
        public const string DeactivateAny = "users.deactivate.any";
    }
}
