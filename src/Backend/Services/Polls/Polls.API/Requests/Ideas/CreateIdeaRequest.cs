namespace Polls.API.Requests.Ideas;

public record CreateIdeaRequest
{
    public required string Title { get; init; }
    public string? Description { get; init; }
}
