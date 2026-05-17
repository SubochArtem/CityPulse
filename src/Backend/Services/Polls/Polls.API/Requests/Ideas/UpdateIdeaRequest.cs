namespace Polls.API.Requests.Ideas;

public record UpdateIdeaRequest
{
    public required string Title { get; init; }
    public string? Description { get; init; }
    public IFormFileCollection? ImagesToAdd { get; init; } 
    public List<Guid>? ImagesToDelete { get; init; }
}
