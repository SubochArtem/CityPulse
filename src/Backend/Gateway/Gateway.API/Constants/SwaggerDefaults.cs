namespace Gateway.API.Constants;

public static class SwaggerDefaults
{
    public const string RoutePrefix = "swagger";
    public const string PathBase = "/swagger";
    public const string AudienceParam = "audience";
    public const string PromptParam = "prompt";
    public const string PromptValue = "login";
    public const string OpenIdScope = "openid";
    public const string ProfileScope = "profile";
    public const string EmailScope = "email";

    public static IReadOnlyList<SwaggerDocument> Documents { get; } =
    [
        new("Users", "users"),
        new("Polls", "polls"),
        new("Feedback", "feedback")
    ];

    public static string DocumentUrl(string name) => $"{PathBase}/{name}/swagger.json";
}

public sealed record SwaggerDocument(string Title, string Name);
