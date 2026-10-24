using Polls.Domain.Polls.Enums;
using Xunit;

namespace Polls.Tests.Unit.Validators.TestData;

public static class CreatePollCommandValidatorTestData
{
    public static TheoryData<PollType> UnknownTypes => new()
    {
        (PollType)99,
        (PollType)(-1)
    };

    public static TheoryData<PollType> DefinedTypes => new()
    {
        PollType.Undefined,
        PollType.UserVote,
        PollType.ManagerVote
    };

    public static TheoryData<TimeSpan> TooShortDurations => new()
    {
        TimeSpan.FromHours(1),
        TimeSpan.FromDays(1),
        TimeSpan.FromDays(2) + TimeSpan.FromHours(23)
    };

    public static TheoryData<TimeSpan> TooLongDurations => new()
    {
        TimeSpan.FromDays(180) + TimeSpan.FromHours(1),
        TimeSpan.FromDays(365)
    };

    public static TheoryData<TimeSpan> ValidDurations => new()
    {
        TimeSpan.FromDays(3) + TimeSpan.FromMinutes(1),
        TimeSpan.FromDays(10),
        TimeSpan.FromDays(179)
    };
}
