using Polls.Domain.Common;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;
using Xunit;

namespace Polls.Tests.Unit.Handlers.Ideas.TestData;

public sealed record IdeaAccessViolationCase(
    string Name,
    Action<Idea> Prepare,
    Func<Idea, Guid> UserIdSelector,
    Func<Idea, Error> ExpectedError)
{
    public override string ToString() => Name;
}

public static class IdeaHandlerTestData
{
    public static TheoryData<IdeaAccessViolationCase> AccessViolations => new()
    {
        new IdeaAccessViolationCase(
            "UserIsNotOwner",
            _ => { },
            _ => Guid.NewGuid(),
            idea => IdeaErrors.NotOwner(idea.Id)),
        new IdeaAccessViolationCase(
            "IdeaIsApproved",
            idea => idea.ApprovalStatus = IdeaApprovalStatus.Approved,
            idea => idea.UserId,
            idea => IdeaErrors.AlreadyApproved(idea.Id)),
        new IdeaAccessViolationCase(
            "PollIsFinished",
            idea => idea.Poll.Status = PollStatus.Finished,
            idea => idea.UserId,
            idea => PollErrors.AlreadyFinished(idea.Poll.Id)),
        new IdeaAccessViolationCase(
            "PollHasEnded",
            idea => idea.Poll.EndsAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            idea => idea.UserId,
            idea => PollErrors.AlreadyFinished(idea.Poll.Id))
    };

    public static TheoryData<IdeaAccessStatus> NotActiveStatuses => new()
    {
        IdeaAccessStatus.Undefined,
        IdeaAccessStatus.Inactive,
        IdeaAccessStatus.RestrictedByContext,
        IdeaAccessStatus.RestrictedByAuthor
    };

    public static TheoryData<IdeaAccessStatus?> AnyFilterStatuses => new()
    {
        null,
        IdeaAccessStatus.Undefined,
        IdeaAccessStatus.Active,
        IdeaAccessStatus.Inactive,
        IdeaAccessStatus.RestrictedByContext,
        IdeaAccessStatus.RestrictedByAuthor
    };

    public static TheoryData<IdeaAccessStatus?> NonActiveFilterStatuses => new()
    {
        null,
        IdeaAccessStatus.Undefined,
        IdeaAccessStatus.Inactive,
        IdeaAccessStatus.RestrictedByContext,
        IdeaAccessStatus.RestrictedByAuthor
    };
}
