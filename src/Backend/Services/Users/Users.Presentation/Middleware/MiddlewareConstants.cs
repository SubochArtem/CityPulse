namespace Users.Presentation.Middleware;

public static class MiddlewareConstants
{
    public const string ContentType = "application/json";
    public const string ProblemExtensionKeys = "errors";
    public const string ExceptionLogTemplate = "Exception at {Method} {Path}{Query}";
}
