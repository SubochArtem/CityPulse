using CityPulse.Contracts.Events.Polls;
using CityPulse.Contracts.Events.Polls.Enums;
using Moq;
using Polls.Application.Jobs;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.TestData;
using Xunit;
using ScheduleJob = Polls.Domain.PollScheduleJob.PollScheduleJob;

namespace Polls.Tests.Unit.Jobs.Tests;

public sealed class PollStatusJobTests : HandlerTestsBase
{
    private readonly PollStatusJob _sut;

    public PollStatusJobTests()
    {
        _sut = new PollStatusJob(UnitOfWork.Object, EventPublisher.Object, Clock.Object);
    }

    private Poll SetupPoll(PollStatus status)
    {
        var poll = PollFakers.Polls().Generate();
        poll.Status = status;
        UnitOfWork.Polls
            .Setup(x => x.GetByIdAsync(poll.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(poll);

        return poll;
    }

    private void VerifyNothingPublished() =>
        EventPublisher.Verify(
            x => x.PublishAsync(It.IsAny<It.IsAnyType>(), It.IsAny<CancellationToken>()),
            Times.Never);

    [Fact]
    public async Task ExecuteAsync_PollDoesNotExist_DoesNothing()
    {
        var pollId = Guid.NewGuid();
        UnitOfWork.Polls
            .Setup(x => x.GetByIdAsync(pollId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Poll?)null);

        await _sut.ExecuteAsync(pollId);

        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        VerifyNothingPublished();
    }

    [Theory]
    [InlineData(PollStatus.Finished)]
    [InlineData(PollStatus.Cancelled)]
    [InlineData(PollStatus.Suspended)]
    [InlineData(PollStatus.Inactive)]
    public async Task ExecuteAsync_PollIsNotActive_DoesNothing(PollStatus status)
    {
        var poll = SetupPoll(status);

        await _sut.ExecuteAsync(poll.Id);

        Assert.Equal(status, poll.Status);
        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        VerifyNothingPublished();
    }

    [Fact]
    public async Task ExecuteAsync_PollIsActive_DeactivatesPollRemovesScheduleAndPublishesEventBeforeSaving()
    {
        var poll = SetupPoll(PollStatus.Active);
        var scheduleEntry = new ScheduleJob { PollId = poll.Id, HangfireJobId = "job-1" };
        var calls = new List<string>();
        UnitOfWork.Polls.Setup(x => x.Update(poll));
        UnitOfWork.PollScheduleJobs
            .Setup(x => x.GetByPollIdAsync(poll.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scheduleEntry);
        UnitOfWork.PollScheduleJobs.Setup(x => x.Delete(scheduleEntry));
        EventPublisher
            .Setup(x => x.PublishAsync(It.IsAny<PollStatusChangedEvent>(), It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("publish"))
            .Returns(Task.CompletedTask);
        UnitOfWork.Mock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("save"))
            .Returns(Task.CompletedTask);

        await _sut.ExecuteAsync(poll.Id);

        Assert.Equal(PollStatus.Inactive, poll.Status);
        Assert.Equal(["publish", "save"], calls);
        UnitOfWork.PollScheduleJobs.Verify(x => x.Delete(scheduleEntry), Times.Once);
        EventPublisher.Verify(
            x => x.PublishAsync(
                It.Is<PollStatusChangedEvent>(e =>
                    e.PollId == poll.Id &&
                    e.CityId == poll.CityId &&
                    e.Status == PollLifecycleStatus.Inactive &&
                    e.OccurredAt == Now),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
