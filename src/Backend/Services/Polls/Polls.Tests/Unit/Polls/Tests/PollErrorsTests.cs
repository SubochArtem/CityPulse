using Polls.Domain.Common.Enums;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;
using Polls.Tests.Unit.Polls.TestData;
using Xunit;

namespace Polls.Tests.Unit.Polls.Tests;

public sealed class PollErrorsTests
{
    private const int MaxDays = 7;

    [Fact]
    public void NotFound_WithId_ReturnsNotFoundErrorMentioningId()
    {
        var id = Guid.NewGuid();

        var error = PollErrors.NotFound(id);

        Assert.Equal(ErrorType.NotFound, error.Type);
        Assert.Contains(id.ToString(), error.Description);
    }

    [Fact]
    public void AlreadyFinished_WithId_ReturnsConflictErrorMentioningId()
    {
        var id = Guid.NewGuid();

        var error = PollErrors.AlreadyFinished(id);

        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.Contains(id.ToString(), error.Description);
    }

    [Fact]
    public void AlreadyExists_WithCityId_ReturnsConflictErrorMentioningCity()
    {
        var cityId = Guid.NewGuid();

        var error = PollErrors.AlreadyExists(cityId);

        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.Contains(cityId.ToString(), error.Description);
    }

    [Fact]
    public void MaxDurationExceeded_WithMaxDays_ReturnsConflictErrorMentioningDays()
    {
        var error = PollErrors.MaxDurationExceeded(MaxDays);

        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.Contains(MaxDays.ToString(), error.Description);
    }

    [Fact]
    public void UpdatePeriodExpired_WithMaxDays_ReturnsConflictErrorMentioningDays()
    {
        var error = PollErrors.UpdatePeriodExpired(MaxDays);

        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.Contains(MaxDays.ToString(), error.Description);
    }

    [Fact]
    public void NotFromUserCity_WithPollId_ReturnsForbiddenErrorMentioningPoll()
    {
        var pollId = Guid.NewGuid();

        var error = PollErrors.NotFromUserCity(pollId);

        Assert.Equal(ErrorType.Forbidden, error.Type);
        Assert.Contains(pollId.ToString(), error.Description);
    }

    [Fact]
    public void NotFromUserCity_WithoutPollId_ReturnsForbiddenError()
    {
        var error = PollErrors.NotFromUserCity();

        Assert.Equal(ErrorType.Forbidden, error.Type);
        Assert.False(string.IsNullOrWhiteSpace(error.Description));
    }

    [Theory]
    [MemberData(nameof(PollErrorsTestData.AllStatuses), MemberType = typeof(PollErrorsTestData))]
    public void InvalidStatus_AnyStatus_ReturnsConflictErrorMentioningStatus(PollStatus status)
    {
        var error = PollErrors.InvalidStatus(status);

        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.Contains(status.ToString(), error.Description);
    }
}
