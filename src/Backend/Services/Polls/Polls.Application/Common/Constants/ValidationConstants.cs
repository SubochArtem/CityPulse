namespace Polls.Application.Common.Constants;

public static class ValidationConstants
{
    public static class City
    {
        public const int MaxTitleLength = 100;
        public const int MaxDescriptionLength = 500;

        public static readonly string TitleRequired = "City title is required";
        public static readonly string IdRequired = "City ID is required";
        public static readonly string TitleTooLong = $"City title must not exceed {MaxTitleLength} characters";
        public static readonly string CoordinatesRequired = "Coordinates are required";
        public static readonly string InvalidStatus = "The specified city status is invalid";
        public static readonly string DescriptionTooLong = $"Description must not exceed {MaxDescriptionLength} characters";
    }

    public static class Coordinates
    {
        public const double MinLatitude = -90;
        public const double MaxLatitude = 90;
        public const double MinLongitude = -180;
        public const double MaxLongitude = 180;

        public static readonly string InvalidLatitude = $"Latitude must be between {MinLatitude} and {MaxLatitude} degrees";
        public static readonly string InvalidLongitude = $"Longitude must be between {MinLongitude} and {MaxLongitude} degrees";
    }

    public static class Poll
    {
        public const int MaxDurationDays = 180;
        public const int MinDurationDays = 3;
        public const int MaxUpdatePeriodDays = 1;
        public const decimal MaxBudgetAmount = 100_000_000;
        public const decimal MinBudgetAmount = 100_000_000;
        public const int MaxTitleLength = 300;
        public const int MaxDescriptionLength = 2000;

        public static readonly string BudgetPositive = $"Budget amount must be greater than {MinBudgetAmount}";
        public static readonly string EndDateInFuture = "End date must be in the future";
        public static readonly string TooLongDuration = $"Poll duration cannot exceed {MaxDurationDays} days";
        public static readonly string TooShortDuration = $"Poll duration must be at least {MinDurationDays} days";
        public static readonly string BudgetTooHigh = $"Budget amount cannot exceed {MaxBudgetAmount:N0}";
        public static readonly string DescriptionTooLong = $"Description must not exceed {MaxDescriptionLength} characters";
        public static readonly string TitleTooLong = $"Poll title must not exceed {MaxTitleLength} characters";
        public static readonly string TitleRequired = "Poll title is required";
        public static readonly string InvalidType = "The specified poll type is invalid";
        public static readonly string InvalidStatus = "The specified poll status is invalid";
        public static readonly string IdRequired = "Poll ID is required";
    }

    public static class Idea
    {
        public const int MaxTitleLength = 200;
        public const int MaxDescriptionLength = 1000;

        public static readonly string TitleRequired = "Idea title is required";
        public static readonly string TitleTooLong = $"Idea title must not exceed {MaxTitleLength} characters";
        public static readonly string DescriptionTooLong = $"Description must not exceed {MaxDescriptionLength} characters";
        public static readonly string UserIdRequired = "User ID is required";
        public static readonly string IdRequired = "Idea ID is required";
        public static readonly string InvalidStatus = "The specified Idea status is invalid";
    }

    public static class Search
    {
        public const int MaxSearchTermLength = 200;
        public static readonly string SearchTermTooLong = $"Search term cannot exceed {MaxSearchTermLength} characters";
    }

    public static class Pagination
    {
        public const int MinPage = 1;
        public const int MinPageSize = 1;
        public const int MaxPageSize = 100;

        public static readonly string FilterRequired = "Filter parameters are required";
        public static readonly string PageInvalid = $"Page number must be at least {MinPage}";
        public static readonly string PageSizeTooSmall = $"Page size must be at least {MinPageSize}";
        public static readonly string PageSizeTooLarge = $"Page size must not exceed {MaxPageSize}";
    }
}
