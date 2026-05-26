namespace Users.Business.Constants;

public static class ValidationConstants
{
    public static class User
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

    public static class Pagination
    {
        public const int MinPageSize = 1;
        public const int MaxPageSize = 100;
        public const int MinPage = 1;
        
        public static readonly string PageInvalid = $"Page number must be at least {MinPage}.";
        public static readonly string PageSizeTooSmall = $"Page size must be at least {MinPageSize}.";
        public static readonly string PageSizeTooLarge = $"Page size must not exceed {MaxPageSize}.";
    }
}
