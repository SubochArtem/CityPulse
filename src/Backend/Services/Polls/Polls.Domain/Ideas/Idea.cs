using Polls.Domain.Common;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Polls;

namespace Polls.Domain.Ideas;

public class Idea : EntityBase
{
    public Guid UserId { get; set; }
    public Guid PollId { get; set; }
    public required Poll Poll { get; set; }
    public IdeaStatus Status { get; set; } = IdeaStatus.InPoll;
    public required string Title { get; set; }
    public string? Description { get; set; }
}
