using Polls.Domain.Ideas.Enums;

namespace Polls.Domain.Ideas;

public class Idea
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PollId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public IdeaStatus Status { get; set; } = IdeaStatus.InPoll;
    public DateTimeOffset MovedToPollAt { get; set; } = DateTimeOffset.UtcNow;
}
