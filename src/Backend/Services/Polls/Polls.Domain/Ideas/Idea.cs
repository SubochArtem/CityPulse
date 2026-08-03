using Polls.Domain.Common;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Images;
using Polls.Domain.Polls;

namespace Polls.Domain.Ideas;

public class Idea : EntityBase
{
    public Guid UserId { get; set; }
    public Guid PollId { get; set; }
    public Poll Poll { get; set; } = null!;
    public IdeaAccessStatus AccessStatus { get; set; } = IdeaAccessStatus.Undefined;
    public IdeaApprovalStatus ApprovalStatus { get; set; } = IdeaApprovalStatus.Undefined;
    public ICollection<IdeaImage> Images { get; set; } = [];
}
