namespace Feedback.Tests.Infrastructure.Constants;

internal static class EnvironmentConstants
{
    public static class Mongo
    {
        public const string Image = "mongo:8";
        public const string DatabasePrefix = "feedback_";
        public const string FeedbackCollection = "feedback";
    }

    public static class Traits
    {
        public const string Category = "Category";
        public const string Integration = "Integration";
    }
}
