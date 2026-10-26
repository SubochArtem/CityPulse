namespace Users.Tests.Infrastructure;

public sealed class ValidationProblemResponse
{
    public string? Title { get; set; }

    public string[] Errors { get; set; } = [];
}
