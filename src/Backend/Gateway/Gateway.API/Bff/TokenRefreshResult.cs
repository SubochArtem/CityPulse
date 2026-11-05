namespace Gateway.API.Bff;

public enum TokenRefreshStatus
{
    Succeeded,
    Rejected,
    Failed
}

public sealed record TokenRefreshResult(TokenRefreshStatus Status, TokenRefreshResponse? Tokens = null)
{
    public static TokenRefreshResult Rejected { get; } = new(TokenRefreshStatus.Rejected);

    public static TokenRefreshResult Failed { get; } = new(TokenRefreshStatus.Failed);

    public static TokenRefreshResult Succeeded(TokenRefreshResponse tokens) =>
        new(TokenRefreshStatus.Succeeded, tokens);
}
