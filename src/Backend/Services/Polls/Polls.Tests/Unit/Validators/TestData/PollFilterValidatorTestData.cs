using Polls.Domain.Polls.Enums;
using Xunit;

namespace Polls.Tests.Unit.Validators.TestData;

public static class PollFilterValidatorTestData
{
    public static TheoryData<PollType> UnknownTypes => new()
    {
        (PollType)99,
        (PollType)(-1)
    };

    public static TheoryData<PollType?> ValidTypes => new()
    {
        null,
        PollType.Undefined,
        PollType.UserVote,
        PollType.ManagerVote
    };

    public static TheoryData<PollStatus> UnknownStatuses => new()
    {
        (PollStatus)99,
        (PollStatus)(-1)
    };

    public static TheoryData<PollStatus?> ValidStatuses => new()
    {
        null,
        PollStatus.Undefined,
        PollStatus.Active,
        PollStatus.Finished,
        PollStatus.Cancelled,
        PollStatus.Suspended,
        PollStatus.Inactive
    };

    public static TheoryData<DateTimeOffset?> EndsBeforeValues => new()
    {
        null,
        DateTimeOffset.MinValue,
        DateTimeOffset.MaxValue
    };
}
