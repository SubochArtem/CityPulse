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
    public AccessStatus AccessStatus { get; set; } = AccessStatus.Undefined;
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Undefined;
    public ICollection<IdeaImage> Images { get; set; } = [];
}
