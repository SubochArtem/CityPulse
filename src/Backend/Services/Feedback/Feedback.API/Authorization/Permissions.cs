namespace Feedback.API.Authorization;

public static class Permissions
{
    public static class Feedback
    {
        public const string ReadAny = "feedback.read.any";
        public const string CreateCity = "feedback.create.city";
        public const string CreateAny = "feedback.create.any";
        public const string UpdateOwn = "feedback.update.own";
        public const string UpdateAny = "feedback.update.any";
        public const string DeleteOwn = "feedback.delete.own";
        public const string DeleteAny = "feedback.delete.any";
    }
}
