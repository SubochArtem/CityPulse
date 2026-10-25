using Polls.Domain.Ideas.Enums;
using Xunit;

namespace Polls.Tests.Unit.Errors.TestData;

public static class IdeaErrorsTestData
{
    public static TheoryData<IdeaAccessStatus> AllAccessStatuses => new()
    {
        IdeaAccessStatus.Undefined,
        IdeaAccessStatus.Active,
        IdeaAccessStatus.Inactive,
        IdeaAccessStatus.RestrictedByContext,
        IdeaAccessStatus.RestrictedByAuthor
    };

    public static TheoryData<IdeaApprovalStatus> AllApprovalStatuses => new()
    {
        IdeaApprovalStatus.Undefined,
        IdeaApprovalStatus.Approved,
        IdeaApprovalStatus.Rejected,
        IdeaApprovalStatus.Deferred,
        IdeaApprovalStatus.Pending
    };
}
