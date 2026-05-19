namespace Users.Business.Constants;

public static class ValidationMessages
{
    public const string IdentityIdRequired = "IdentityId is required.";

    public const string IdentityIdInvalidFormat = "IdentityId must be in format '<provider>|<provider_user_id>'.";

    public const string NicknameRequired = "Nickname cannot be empty.";

    public const string NicknameTooShort = "Nickname must be at least 3 characters.";

    public const string NicknameTooLong = "Nickname cannot exceed 30 characters.";

    public const string NicknameInvalidCharacters = "Nickname can only contain letters, numbers, dots, plus, underscores and hyphens.";

    public const string NicknameConsecutiveSpecialCharacters = "Nickname cannot contain consecutive special characters.";
}
