using Polls.Domain.Ideas.Enums;
using Xunit;

namespace Polls.Tests.Unit.Validators.TestData;

public static class IdeaFilterValidatorTestData
{
    public static TheoryData<IdeaAccessStatus> UnknownAccessStatuses => new()
    {
        (IdeaAccessStatus)99,
        (IdeaAccessStatus)(-1)
    };

    public static TheoryData<IdeaAccessStatus?> ValidAccessStatuses => new()
    {
        null,
        IdeaAccessStatus.Undefined,
        IdeaAccessStatus.Active,
        IdeaAccessStatus.Inactive,
        IdeaAccessStatus.RestrictedByContext,
        IdeaAccessStatus.RestrictedByAuthor
    };

    public static TheoryData<IdeaApprovalStatus> UnknownApprovalStatuses => new()
    {
        (IdeaApprovalStatus)99,
        (IdeaApprovalStatus)(-1)
    };

    public static TheoryData<IdeaApprovalStatus?> ValidApprovalStatuses => new()
    {
        null,
        IdeaApprovalStatus.Undefined,
        IdeaApprovalStatus.Approved,
        IdeaApprovalStatus.Rejected,
        IdeaApprovalStatus.Deferred,
        IdeaApprovalStatus.Pending
    };

    public static TheoryData<Guid?> PollIds => new()
    {
        null,
        Guid.Empty,
        Guid.Parse("6b1f2c1e-4d8a-4f6e-9a57-1c2d3e4f5a6b")
    };
}
