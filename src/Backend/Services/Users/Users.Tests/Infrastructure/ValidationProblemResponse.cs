namespace Users.Tests.Infrastructure;

public sealed class ValidationProblemResponse
{
    public string? Title { get; set; }

    public Dictionary<string, string[]> Errors { get; set; } = [];
}
