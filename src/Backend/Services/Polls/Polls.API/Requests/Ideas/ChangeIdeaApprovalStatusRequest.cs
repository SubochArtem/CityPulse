using Polls.Domain.Ideas.Enums;

namespace Polls.API.Requests.Ideas;

public class ChangeIdeaApprovalStatusRequest
{
    public required IdeaApprovalStatus NewApprovalStatus {get; init; }
}
