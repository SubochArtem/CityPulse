namespace Users.Business.Constants;

public static class ValidationConstants
{
    public const int MinNicknameLength = 3;
    public const int MaxNicknameLength = 30;
    public const string NicknamePattern = "^[a-zA-Z0-9._+_-]+$";
    public const string NicknameConsecutivePattern = "^(?!.*[.+_-]{2})";
    public const string IdentityIdPattern = @"^[a-zA-Z0-9_-]+\|[a-zA-Z0-9@._-]+$";
    
    public const string IdentityIdRequired = "IdentityId is required.";

    public const string IdentityIdInvalidFormat = "IdentityId must be in format '<provider>|<provider_user_id>'.";

    public const string NicknameRequired = "Nickname cannot be empty.";

    public static readonly string NicknameTooShort = $"Nickname must be at least {MinNicknameLength} characters.";

    public static readonly string NicknameTooLong = $"Nickname cannot exceed {MaxNicknameLength} characters.";

    public const string NicknameInvalidCharacters = "Nickname can only contain letters, numbers, dots, plus, underscores and hyphens.";

    public const string NicknameConsecutiveSpecialCharacters = "Nickname cannot contain consecutive special characters.";
}
