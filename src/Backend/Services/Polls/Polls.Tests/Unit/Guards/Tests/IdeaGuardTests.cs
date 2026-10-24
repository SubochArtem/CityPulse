using Polls.Application.Ideas.Guards;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;
using Polls.Tests.TestData;
using Polls.Tests.Unit.Guards.TestData;
using Xunit;
using MediatRUnit = MediatR.Unit;

namespace Polls.Tests.Unit.Guards.Tests;

public sealed class IdeaGuardTests
{
    private static Idea BuildIdea(IdeaApprovalStatus approvalStatus = IdeaApprovalStatus.Pending)
    {
        var idea = IdeaFakers.Ideas().Generate();
        idea.ApprovalStatus = approvalStatus;

        return idea;
    }

    [Fact]
    public void Validate_NoChecksApplied_ReturnsSuccess()
    {
        var idea = BuildIdea();

        var result = IdeaGuard.For(idea).Validate();

        Assert.True(result.IsSuccess);
        Assert.Equal(MediatRUnit.Value, result.Value);
    }

    [Fact]
    public void IsOwner_UserIsOwner_ReturnsSuccess()
    {
        var idea = BuildIdea();

        var result = IdeaGuard.For(idea).IsOwner(idea.UserId).Validate();

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void IsOwner_UserIsNotOwner_ReturnsNotOwnerError()
    {
        var idea = BuildIdea();

        var result = IdeaGuard.For(idea).IsOwner(Guid.NewGuid()).Validate();

        Assert.False(result.IsSuccess);
        Assert.Equal(IdeaErrors.NotOwner(idea.Id), result.Error);
    }

    [Fact]
    public void IsNotApproved_IdeaIsApproved_ReturnsAlreadyApprovedError()
    {
        var idea = BuildIdea(IdeaApprovalStatus.Approved);

        var result = IdeaGuard.For(idea).IsNotApproved().Validate();

        Assert.False(result.IsSuccess);
        Assert.Equal(IdeaErrors.AlreadyApproved(idea.Id), result.Error);
    }

    [Theory]
    [MemberData(nameof(IdeaGuardTestData.NotApprovedStatuses), MemberType = typeof(IdeaGuardTestData))]
    public void IsNotApproved_IdeaIsNotApproved_ReturnsSuccess(IdeaApprovalStatus approvalStatus)
    {
        var idea = BuildIdea(approvalStatus);

        var result = IdeaGuard.For(idea).IsNotApproved().Validate();

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Validate_AllChecksPass_ReturnsSuccess()
    {
        var idea = BuildIdea();

        var result = IdeaGuard.For(idea).IsOwner(idea.UserId).IsNotApproved().Validate();

        Assert.True(result.IsSuccess);
        Assert.Equal(MediatRUnit.Value, result.Value);
    }

    [Fact]
    public void Validate_OwnerCheckFailsBeforeApprovalCheck_ReturnsNotOwnerError()
    {
        var idea = BuildIdea(IdeaApprovalStatus.Approved);

        var result = IdeaGuard.For(idea).IsOwner(Guid.NewGuid()).IsNotApproved().Validate();

        Assert.Equal(IdeaErrors.NotOwner(idea.Id), result.Error);
    }

    [Fact]
    public void Validate_ApprovalCheckFailsBeforeOwnerCheck_ReturnsAlreadyApprovedError()
    {
        var idea = BuildIdea(IdeaApprovalStatus.Approved);

        var result = IdeaGuard.For(idea).IsNotApproved().IsOwner(Guid.NewGuid()).Validate();

        Assert.Equal(IdeaErrors.AlreadyApproved(idea.Id), result.Error);
    }

    [Fact]
    public void Validate_SecondCheckFailsAfterFirstPassed_ReturnsSecondCheckError()
    {
        var idea = BuildIdea(IdeaApprovalStatus.Approved);

        var result = IdeaGuard.For(idea).IsOwner(idea.UserId).IsNotApproved().Validate();

        Assert.Equal(IdeaErrors.AlreadyApproved(idea.Id), result.Error);
    }
}
