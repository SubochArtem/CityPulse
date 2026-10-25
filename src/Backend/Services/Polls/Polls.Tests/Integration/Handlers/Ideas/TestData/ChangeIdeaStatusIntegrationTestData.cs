using Polls.Domain.Ideas.Enums;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Ideas.TestData;

public static class ChangeIdeaStatusIntegrationTestData
{
    public static TheoryData<IdeaAccessStatus, IdeaAccessStatus> AccessTransitions => new()
    {
        { IdeaAccessStatus.Active, IdeaAccessStatus.Inactive },
        { IdeaAccessStatus.Active, IdeaAccessStatus.RestrictedByContext },
        { IdeaAccessStatus.Active, IdeaAccessStatus.RestrictedByAuthor },
        { IdeaAccessStatus.Inactive, IdeaAccessStatus.Active },
        { IdeaAccessStatus.RestrictedByContext, IdeaAccessStatus.Active },
        { IdeaAccessStatus.RestrictedByAuthor, IdeaAccessStatus.Active }
    };

    public static TheoryData<IdeaApprovalStatus, IdeaApprovalStatus> ApprovalTransitions => new()
    {
        { IdeaApprovalStatus.Pending, IdeaApprovalStatus.Approved },
        { IdeaApprovalStatus.Pending, IdeaApprovalStatus.Rejected },
        { IdeaApprovalStatus.Pending, IdeaApprovalStatus.Deferred },
        { IdeaApprovalStatus.Approved, IdeaApprovalStatus.Pending },
        { IdeaApprovalStatus.Rejected, IdeaApprovalStatus.Approved },
        { IdeaApprovalStatus.Deferred, IdeaApprovalStatus.Rejected }
    };
}
