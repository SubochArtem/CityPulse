using Polls.Domain.Ideas.Enums;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Ideas.TestData;

public static class UpdateIdeaIntegrationTestData
{
    public static TheoryData<IdeaApprovalStatus> EditableApprovalStatuses => new()
    {
        IdeaApprovalStatus.Pending,
        IdeaApprovalStatus.Rejected,
        IdeaApprovalStatus.Deferred
    };
}
