using Polls.Domain.Polls.Enums;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Polls.TestData;

public static class ChangePollStatusIntegrationTestData
{
    public static TheoryData<PollStatus> NotSupportedStatuses => new()
    {
        PollStatus.Undefined,
        PollStatus.Finished,
        PollStatus.Cancelled,
        PollStatus.Inactive
    };
}
