namespace Gateway.API.Bff;

public enum SessionTokenStatus
{
    Available,
    Expired,
    Unavailable
}

public sealed record SessionToken(SessionTokenStatus Status, string? AccessToken = null)
{
    public static SessionToken Expired { get; } = new(SessionTokenStatus.Expired);

    public static SessionToken Unavailable { get; } = new(SessionTokenStatus.Unavailable);

    public static SessionToken Available(string accessToken) => new(SessionTokenStatus.Available, accessToken);
}
