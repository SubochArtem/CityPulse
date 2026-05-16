using Polls.Application.Polls.DTOs;

namespace Polls.Application.Ideas.DTOs;

public class IdeaWithPollDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid PollId { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public int Status { get; init; }
    public required PollDto Poll { get; init; }
}
