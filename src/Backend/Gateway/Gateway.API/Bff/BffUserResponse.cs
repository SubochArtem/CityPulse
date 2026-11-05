namespace Gateway.API.Bff;

public sealed record BffUserResponse(
    bool IsAuthenticated,
    string? UserId,
    IReadOnlyList<string> Permissions,
    string? LogoutUrl)
{
    public static BffUserResponse Anonymous { get; } = new(false, null, [], null);
}
