namespace Polls.Application.Common.Constants;

public static class ValidationMessages
{
    public static class City
    {
        public const int MaxNameLength = 100;
        public const int MaxDescriptionLength = 500;

        public static readonly string NameRequired = "City name is required";
        public static readonly string IdRequired = "City ID is required";
        public static readonly string NameTooLong = $"City name must not exceed {MaxNameLength} characters";
        public static readonly string CoordinatesRequired = "Coordinates are required";

        public static readonly string DescriptionTooLong =
            $"Description must not exceed {MaxDescriptionLength} characters";
    }

    public static class Coordinates
    {
        public const double MinLatitude = -90;
        public const double MaxLatitude = 90;
        public const double MinLongitude = -180;
        public const double MaxLongitude = 180;

        public static readonly string InvalidLatitude =
            $"Latitude must be between {MinLatitude} and {MaxLatitude} degrees";

        public static readonly string InvalidLongitude =
            $"Longitude must be between {MinLongitude} and {MaxLongitude} degrees";
    }
}
