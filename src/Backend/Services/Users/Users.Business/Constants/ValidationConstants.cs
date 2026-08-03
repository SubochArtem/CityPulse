namespace Users.Business.Constants;

public static class ValidationConstants
{
    public static class Nickname
    {
        public const int MinLength = 3;
        public const int MaxLength = 30;
        public const string Pattern = "^[a-zA-Z0-9._+_-]+$";
        public const string ConsecutivePattern = "^(?!.*[.+_-]{2})";
        
        public const string Required = "Nickname cannot be empty";
        public const string InvalidCharacters = "Nickname can only contain letters, numbers, dots, plus, underscores and hyphens";
        public const string ConsecutiveSpecialCharacters = "Nickname cannot contain consecutive special characters";
        public static readonly string TooShort = $"Nickname must be at least {MinLength} characters";
        public static readonly string TooLong = $"Nickname cannot exceed {MaxLength} characters";
    }

    public static class IdentityId
    {
        public const string Pattern = @"^[a-zA-Z0-9_-]+\|[a-zA-Z0-9@._-]+$";

        public const string Required = "IdentityId is required";
        public const string InvalidFormat = "IdentityId must be in format '<provider>|<provider_user_id>'";
    }
    
    public static class Pagination
    {
        public const int MinPageSize = 1;
        public const int MaxPageSize = 100;
        public const int MinPage = 1;
        
        public static readonly string PageInvalid = $"Page number must be at least {MinPage}";
        public static readonly string PageSizeTooSmall = $"Page size must be at least {MinPageSize}";
        public static readonly string PageSizeTooLarge = $"Page size must not exceed {MaxPageSize}";
    }
}
