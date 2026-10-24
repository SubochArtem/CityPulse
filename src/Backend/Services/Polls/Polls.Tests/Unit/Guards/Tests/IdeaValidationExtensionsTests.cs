using Polls.Application.Ideas.Guards;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;
using Polls.Tests.TestData;
using Polls.Tests.Unit.Guards.TestData;
using Xunit;

namespace Polls.Tests.Unit.Guards.Tests;

public sealed class IdeaValidationExtensionsTests
{
    private static Idea BuildIdea(
        IdeaApprovalStatus approvalStatus = IdeaApprovalStatus.Pending,
        PollStatus pollStatus = PollStatus.Active,
        TimeSpan? pollEndsIn = null)
    {
        var idea = IdeaFakers.IdeaWithPoll();
        idea.ApprovalStatus = approvalStatus;
        idea.Poll.Status = pollStatus;
        idea.Poll.EndsAt = DateTimeOffset.UtcNow.Add(pollEndsIn ?? TimeSpan.FromDays(10));

        return idea;
    }

    [Fact]
    public void ValidateIdeaAccess_OwnerOfPendingIdeaInOpenPoll_ReturnsSuccess()
    {
        var idea = BuildIdea();

        var result = idea.ValidateIdeaAccess(idea.UserId);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateIdeaAccess_UserIsNotOwner_ReturnsNotOwnerError()
    {
        var idea = BuildIdea();

        var result = idea.ValidateIdeaAccess(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(IdeaErrors.NotOwner(idea.Id), result.Error);
    }

    [Fact]
    public void ValidateIdeaAccess_IdeaIsApproved_ReturnsAlreadyApprovedError()
    {
        var idea = BuildIdea(IdeaApprovalStatus.Approved);

        var result = idea.ValidateIdeaAccess(idea.UserId);

        Assert.False(result.IsSuccess);
        Assert.Equal(IdeaErrors.AlreadyApproved(idea.Id), result.Error);
    }

    [Theory]
    [MemberData(nameof(PollGuardTestData.NotActiveStatuses), MemberType = typeof(PollGuardTestData))]
    public void ValidateIdeaAccess_PollIsNotActive_ReturnsAlreadyFinishedError(PollStatus status)
    {
        var idea = BuildIdea(pollStatus: status);

        var result = idea.ValidateIdeaAccess(idea.UserId);

        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.AlreadyFinished(idea.Poll.Id), result.Error);
    }

    [Fact]
    public void ValidateIdeaAccess_PollHasEnded_ReturnsAlreadyFinishedError()
    {
        var idea = BuildIdea(pollEndsIn: TimeSpan.FromMinutes(-1));

        var result = idea.ValidateIdeaAccess(idea.UserId);

        Assert.Equal(PollErrors.AlreadyFinished(idea.Poll.Id), result.Error);
    }

    [Fact]
    public void ValidateIdeaAccess_IdeaAndPollAreBothInvalid_ReturnsIdeaErrorFirst()
    {
        var idea = BuildIdea(pollStatus: PollStatus.Finished);

        var result = idea.ValidateIdeaAccess(Guid.NewGuid());

        Assert.Equal(IdeaErrors.NotOwner(idea.Id), result.Error);
    }

    [Fact]
    public void ValidateIdeaAccess_ApprovedIdeaInFinishedPoll_ReturnsAlreadyApprovedError()
    {
        var idea = BuildIdea(IdeaApprovalStatus.Approved, PollStatus.Finished);

        var result = idea.ValidateIdeaAccess(idea.UserId);

        Assert.Equal(IdeaErrors.AlreadyApproved(idea.Id), result.Error);
    }
}
