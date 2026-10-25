using Polls.Domain.Polls.Enums;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Polls.TestData;

public static class GetPollByIdIntegrationTestData
{
    public static TheoryData<PollStatus> NonActiveStatuses => new()
    {
        PollStatus.Suspended,
        PollStatus.Finished,
        PollStatus.Cancelled,
        PollStatus.Inactive
    };
}
