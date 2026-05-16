namespace Polls.Application.Ideas.DTOs;

public class IdeaDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid PollId { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public int Status { get; init; }
}
