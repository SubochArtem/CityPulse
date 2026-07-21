namespace Users.Presentation.Middleware;

public static class MiddlewareExceptionMessages
{
    public static class Titles
    {
        public const string UserNotFound = "User Not Found";
        public const string UserAlreadyExists = "User Already Exists";
        public const string ValidationFailed = "Validation Failed";
        public const string Unauthorized = "Unauthorized";
        public const string BadRequest = "Bad Request";
        public const string IdentityProviderError = "Identity Provider Error";
        public const string InternalServerError = "Internal Server Error";
        public const string CityNotFound = "City Not Found";
        public const string CityNotActive = "City Not Active";
        public const string CitiesServiceUnavailable = "Cities Service Unavailable";
        public const string CitiesServiceTimeout = "Cities Service Timeout";
        public const string WebhookEventIgnored = "Webhook Event Ignored";
    }

    public static class Details
    {
        public const string UnexpectedError = "An unexpected error occurred.";
        public const string IdentityProviderCommunicationError = "An error occurred while communicating with the identity provider.";
        public const string CityNotFound = "City not found.";
        public const string CitiesServiceUnavailable = "Cities service is currently unavailable.";
        public const string CitiesServiceTimeout = "Cities service request timed out.";
    }
}
