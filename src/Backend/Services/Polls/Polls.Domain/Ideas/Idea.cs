using Polls.Domain.Common;
using Polls.Domain.Ideas.Enums;

namespace Polls.Domain.Ideas;

public class Idea : EntityBase
{
    public Guid UserId { get; set; }
    public Guid PollId { get; set; }
    public IdeaStatus Status { get; set; } = IdeaStatus.InPoll;
}
