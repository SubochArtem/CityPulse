using Polls.Domain.Ideas.Enums;

namespace Polls.Application.Common.Models;

public class IdeaFilter : BaseFilter
{
    public Guid? PollId { get; set; }
    public IdeaAccessStatus? AccessStatus { get; set; }
    public IdeaApprovalStatus? ApprovalStatus { get; set; }
}
