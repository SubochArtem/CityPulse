using Polls.Domain.Ideas.Enums;
using Xunit;

namespace Polls.Tests.Unit.Guards.TestData;

public static class IdeaGuardTestData
{
    public static TheoryData<IdeaApprovalStatus> NotApprovedStatuses => new()
    {
        IdeaApprovalStatus.Undefined,
        IdeaApprovalStatus.Rejected,
        IdeaApprovalStatus.Deferred,
        IdeaApprovalStatus.Pending
    };
}
