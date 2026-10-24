using Polls.Domain.Ideas.Enums;
using Polls.Domain.Polls.Enums;
using Xunit;

namespace Polls.Tests.Unit.Handlers.Polls.TestData;

public enum PollFailingStep
{
    SavePoll,
    UpdateIdeas
}

public static class ChangePollStatusTestData
{
    public static TheoryData<PollStatus, PollStatus, IdeaAccessStatus, IdeaAccessStatus> Transitions => new()
    {
        {
            PollStatus.Suspended, PollStatus.Active,
            IdeaAccessStatus.RestrictedByContext, IdeaAccessStatus.Active
        },
        {
            PollStatus.Active, PollStatus.Suspended,
            IdeaAccessStatus.Active, IdeaAccessStatus.RestrictedByContext
        }
    };

    public static TheoryData<PollStatus> UnsupportedStatuses => new()
    {
        PollStatus.Undefined,
        PollStatus.Finished,
        PollStatus.Cancelled,
        PollStatus.Inactive,
        (PollStatus)99
    };

    public static TheoryData<PollFailingStep> FailingSteps => new()
    {
        PollFailingStep.SavePoll,
        PollFailingStep.UpdateIdeas
    };
}
