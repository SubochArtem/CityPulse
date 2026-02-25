namespace Users.Business.Constants;

public static class ValidationMessages
{
    public const string IdentityIdRequired =
        "IdentityId is required.";

    public const string IdentityIdInvalidFormat =
        "IdentityId must be in format '<provider>|<provider_user_id>'.";
}
