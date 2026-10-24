using Polls.Domain.Polls.Enums;
using Xunit;

namespace Polls.Tests.Unit.Errors.TestData;

public static class PollErrorsTestData
{
    public static TheoryData<PollStatus> AllStatuses => new()
    {
        PollStatus.Undefined,
        PollStatus.Active,
        PollStatus.Finished,
        PollStatus.Cancelled,
        PollStatus.Suspended,
        PollStatus.Inactive
    };
}
