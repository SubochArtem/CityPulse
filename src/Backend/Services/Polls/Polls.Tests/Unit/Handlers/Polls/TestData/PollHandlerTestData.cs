using Polls.Application.Common.Constants;
using Polls.Domain.Common;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;
using Xunit;

namespace Polls.Tests.Unit.Handlers.Polls.TestData;

public sealed record PollRestrictionCase(
    string Name,
    Action<Poll> Prepare,
    Func<Poll, Error> ExpectedError)
{
    public override string ToString() => Name;
}

public static class PollHandlerTestData
{
    public static TheoryData<PollRestrictionCase> Restrictions => new()
    {
        new PollRestrictionCase(
            "PollIsFinished",
            poll => poll.Status = PollStatus.Finished,
            poll => PollErrors.AlreadyFinished(poll.Id)),
        new PollRestrictionCase(
            "PollHasEnded",
            poll => poll.EndsAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            poll => PollErrors.AlreadyFinished(poll.Id)),
        new PollRestrictionCase(
            "EditWindowHasExpired",
            poll => poll.CreatedAt = DateTimeOffset.UtcNow.AddDays(-(ValidationConstants.Poll.MaxUpdatePeriodDays + 1)),
            _ => PollErrors.UpdatePeriodExpired(ValidationConstants.Poll.MaxUpdatePeriodDays))
    };

    public static TheoryData<PollType> PollTypes => new()
    {
        PollType.UserVote,
        PollType.ManagerVote
    };

    public static TheoryData<PollStatus?> AnyFilterStatuses => new()
    {
        null,
        PollStatus.Undefined,
        PollStatus.Active,
        PollStatus.Finished,
        PollStatus.Cancelled,
        PollStatus.Suspended,
        PollStatus.Inactive
    };

    public static TheoryData<PollStatus?> NonActiveFilterStatuses => new()
    {
        null,
        PollStatus.Undefined,
        PollStatus.Finished,
        PollStatus.Cancelled,
        PollStatus.Suspended,
        PollStatus.Inactive
    };
}
