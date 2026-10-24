using Polls.Domain.Polls.Enums;
using Xunit;

namespace Polls.Tests.Unit.Polls.TestData;

public static class PollHandlerTestData
{
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
