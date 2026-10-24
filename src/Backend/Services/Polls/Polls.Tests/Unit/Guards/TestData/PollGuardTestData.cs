using Polls.Domain.Polls.Enums;
using Xunit;

namespace Polls.Tests.Unit.Guards.TestData;

public static class PollGuardTestData
{
    public static TheoryData<PollStatus> NotActiveStatuses => new()
    {
        PollStatus.Undefined,
        PollStatus.Finished,
        PollStatus.Cancelled,
        PollStatus.Suspended,
        PollStatus.Inactive
    };

    public static TheoryData<TimeSpan> ExpiredEditWindowAges => new()
    {
        TimeSpan.FromHours(25),
        TimeSpan.FromDays(7)
    };

    public static TheoryData<TimeSpan> ValidEditWindowAges => new()
    {
        TimeSpan.Zero,
        TimeSpan.FromHours(1),
        TimeSpan.FromHours(12)
    };

    public static TheoryData<TimeSpan> ValidDurations => new()
    {
        TimeSpan.FromDays(1),
        TimeSpan.FromDays(100),
        TimeSpan.FromDays(180)
    };

    public static TheoryData<TimeSpan> TooLongDurations => new()
    {
        TimeSpan.FromDays(180) + TimeSpan.FromMinutes(1),
        TimeSpan.FromDays(365)
    };
}
