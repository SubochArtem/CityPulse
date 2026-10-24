using Polls.Application.Common.Constants;
using Polls.Application.Polls.Guards;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;
using Polls.Tests.TestData;
using Polls.Tests.Unit.Guards.TestData;
using Xunit;
using MediatRUnit = MediatR.Unit;

namespace Polls.Tests.Unit.Guards.Tests;

public sealed class PollGuardTests
{
    private static Poll BuildPoll(
        PollStatus status = PollStatus.Active,
        TimeSpan? endsIn = null,
        TimeSpan? age = null)
    {
        var poll = PollFakers.Polls().Generate();
        poll.Status = status;
        poll.EndsAt = DateTimeOffset.UtcNow.Add(endsIn ?? TimeSpan.FromDays(10));
        poll.CreatedAt = DateTimeOffset.UtcNow.Subtract(age ?? TimeSpan.FromHours(1));

        return poll;
    }

    [Fact]
    public void Validate_NoChecksApplied_ReturnsSuccess()
    {
        var poll = BuildPoll();

        var result = PollGuard.For(poll).Validate();

        Assert.True(result.IsSuccess);
        Assert.Equal(MediatRUnit.Value, result.Value);
    }

    [Fact]
    public void IsNotFinished_PollIsActiveAndEndsInFuture_ReturnsSuccess()
    {
        var poll = BuildPoll();

        var result = PollGuard.For(poll).IsNotFinished().Validate();

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [MemberData(nameof(PollGuardTestData.NotActiveStatuses), MemberType = typeof(PollGuardTestData))]
    public void IsNotFinished_PollIsNotActive_ReturnsAlreadyFinishedError(PollStatus status)
    {
        var poll = BuildPoll(status);

        var result = PollGuard.For(poll).IsNotFinished().Validate();

        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.AlreadyFinished(poll.Id), result.Error);
    }

    [Fact]
    public void IsNotFinished_ActivePollHasAlreadyEnded_ReturnsAlreadyFinishedError()
    {
        var poll = BuildPoll(endsIn: TimeSpan.FromMinutes(-1));

        var result = PollGuard.For(poll).IsNotFinished().Validate();

        Assert.Equal(PollErrors.AlreadyFinished(poll.Id), result.Error);
    }

    [Theory]
    [MemberData(nameof(PollGuardTestData.ValidEditWindowAges), MemberType = typeof(PollGuardTestData))]
    public void EditWindowNotExpired_PollIsCreatedWithinWindow_ReturnsSuccess(TimeSpan age)
    {
        var poll = BuildPoll(age: age);

        var result = PollGuard.For(poll).EditWindowNotExpired().Validate();

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [MemberData(nameof(PollGuardTestData.ExpiredEditWindowAges), MemberType = typeof(PollGuardTestData))]
    public void EditWindowNotExpired_PollIsCreatedBeforeWindow_ReturnsUpdatePeriodExpiredError(TimeSpan age)
    {
        var poll = BuildPoll(age: age);

        var result = PollGuard.For(poll).EditWindowNotExpired().Validate();

        Assert.False(result.IsSuccess);
        Assert.Equal(
            PollErrors.UpdatePeriodExpired(ValidationConstants.Poll.MaxUpdatePeriodDays),
            result.Error);
    }

    [Theory]
    [MemberData(nameof(PollGuardTestData.ValidDurations), MemberType = typeof(PollGuardTestData))]
    public void DurationIsValid_DurationIsWithinLimit_ReturnsSuccess(TimeSpan duration)
    {
        var poll = BuildPoll();

        var result = PollGuard.For(poll).DurationIsValid(poll.CreatedAt.Add(duration)).Validate();

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [MemberData(nameof(PollGuardTestData.TooLongDurations), MemberType = typeof(PollGuardTestData))]
    public void DurationIsValid_DurationExceedsLimit_ReturnsMaxDurationExceededError(TimeSpan duration)
    {
        var poll = BuildPoll();

        var result = PollGuard.For(poll).DurationIsValid(poll.CreatedAt.Add(duration)).Validate();

        Assert.False(result.IsSuccess);
        Assert.Equal(
            PollErrors.MaxDurationExceeded(ValidationConstants.Poll.MaxDurationDays),
            result.Error);
    }

    [Fact]
    public void Validate_AllChecksPass_ReturnsSuccess()
    {
        var poll = BuildPoll();

        var result = PollGuard.For(poll)
            .IsNotFinished()
            .EditWindowNotExpired()
            .DurationIsValid(poll.EndsAt)
            .Validate();

        Assert.True(result.IsSuccess);
        Assert.Equal(MediatRUnit.Value, result.Value);
    }

    [Fact]
    public void Validate_FinishedCheckFailsBeforeEditWindowCheck_ReturnsAlreadyFinishedError()
    {
        var poll = BuildPoll(PollStatus.Finished, age: TimeSpan.FromDays(7));

        var result = PollGuard.For(poll).IsNotFinished().EditWindowNotExpired().Validate();

        Assert.Equal(PollErrors.AlreadyFinished(poll.Id), result.Error);
    }

    [Fact]
    public void Validate_EditWindowCheckFailsBeforeFinishedCheck_ReturnsUpdatePeriodExpiredError()
    {
        var poll = BuildPoll(PollStatus.Finished, age: TimeSpan.FromDays(7));

        var result = PollGuard.For(poll).EditWindowNotExpired().IsNotFinished().Validate();

        Assert.Equal(
            PollErrors.UpdatePeriodExpired(ValidationConstants.Poll.MaxUpdatePeriodDays),
            result.Error);
    }

    [Fact]
    public void Validate_LaterCheckFailsAfterEarlierChecksPassed_ReturnsLaterCheckError()
    {
        var poll = BuildPoll();
        var tooFarEndsAt = poll.CreatedAt.AddDays(ValidationConstants.Poll.MaxDurationDays + 1);

        var result = PollGuard.For(poll)
            .IsNotFinished()
            .EditWindowNotExpired()
            .DurationIsValid(tooFarEndsAt)
            .Validate();

        Assert.Equal(
            PollErrors.MaxDurationExceeded(ValidationConstants.Poll.MaxDurationDays),
            result.Error);
    }
}
