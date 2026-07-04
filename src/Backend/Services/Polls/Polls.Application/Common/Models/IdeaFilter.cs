using Polls.Domain.Ideas.Enums;

namespace Polls.Application.Common.Models;

public class IdeaFilter : BaseFilter
{
    public Guid? PollId { get; set; }
    public AccessStatus? AccessStatus { get; set; }
    public ApprovalStatus? ApprovalStatus { get; set; }
}
