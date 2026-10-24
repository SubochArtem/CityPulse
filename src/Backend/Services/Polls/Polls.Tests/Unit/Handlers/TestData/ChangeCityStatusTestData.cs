using Polls.Domain.Cities.Enums;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Polls.Enums;
using Xunit;

namespace Polls.Tests.Unit.Handlers.TestData;

public enum FailingStep
{
    SaveCity,
    UpdatePolls,
    UpdateIdeas
}

public static class ChangeCityStatusTestData
{
    public static TheoryData<CityStatus, CityStatus, PollStatus, PollStatus, IdeaAccessStatus, IdeaAccessStatus>
        Transitions => new()
    {
        {
            CityStatus.Inactive, CityStatus.Active,
            PollStatus.Suspended, PollStatus.Active,
            IdeaAccessStatus.RestrictedByContext, IdeaAccessStatus.Active
        },
        {
            CityStatus.Active, CityStatus.Inactive,
            PollStatus.Active, PollStatus.Suspended,
            IdeaAccessStatus.Active, IdeaAccessStatus.RestrictedByContext
        }
    };

    public static TheoryData<CityStatus> UnsupportedStatuses => new()
    {
        CityStatus.Undefined,
        (CityStatus)99
    };

    public static TheoryData<FailingStep> FailingSteps => new()
    {
        FailingStep.SaveCity,
        FailingStep.UpdatePolls,
        FailingStep.UpdateIdeas
    };
}
