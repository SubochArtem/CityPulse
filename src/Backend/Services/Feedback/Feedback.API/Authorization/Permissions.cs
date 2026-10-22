namespace Feedback.API.Authorization;

public static class Permissions
{
    public const string ClaimType = "permissions";

    public static class Feedback
    {
        public const string ReadAny = "feedback.read.any";
        public const string CreateAny = "feedback.create.any";
        public const string UpdateOwn = "feedback.update.own";
        public const string DeleteOwn = "feedback.delete.own";
        public const string DeleteAny = "feedback.delete.any";
    }
}
