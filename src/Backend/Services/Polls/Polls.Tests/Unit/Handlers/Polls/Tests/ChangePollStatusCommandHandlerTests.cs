using Microsoft.Extensions.Logging;
using Moq;
using Polls.Application.Common.Interfaces;
using Polls.Application.Polls.Commands.ChangeStatus;
using Polls.Domain.Common;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.TestData;
using Polls.Tests.Unit.Handlers.Polls.TestData;
using Xunit;

namespace Polls.Tests.Unit.Handlers.Polls.Tests;

public sealed class ChangePollStatusCommandHandlerTests : HandlerTestsBase
{
    private const string UpdateStep = "update";
    private const string SaveStep = "save";
    private const string IdeasStep = "ideas";
    private const string CommitStep = "commit";
    private const string DatabaseFailureMessage = "database failure";

    private readonly Mock<ILogger<ChangePollStatusCommandHandler>> _logger = new();
    private readonly ChangePollStatusCommandHandler _sut;

    public ChangePollStatusCommandHandlerTests()
    {
        _sut = new ChangePollStatusCommandHandler(UnitOfWork.Object, _logger.Object);
    }

    private void SetupPoll(Poll? poll, Guid id) =>
        UnitOfWork.Polls
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(poll);

    private Mock<IUnitOfWorkTransaction> SetupFlow(
        Poll poll,
        IdeaAccessStatus ideaSource,
        IdeaAccessStatus ideaTarget,
        List<string>? calls = null,
        PollFailingStep? failingStep = null,
        Exception? failure = null)
    {
        calls ??= [];
        SetupPoll(poll, poll.Id);

        UnitOfWork.Polls
            .Setup(x => x.Update(poll))
            .Callback(() => calls.Add(UpdateStep));

        var save = UnitOfWork.Mock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()));
        if (failingStep == PollFailingStep.SavePoll)
            save.ThrowsAsync(failure!);
        else
            save.Callback(() => calls.Add(SaveStep)).Returns(Task.CompletedTask);

        var ideas = UnitOfWork.Ideas.Setup(x => x.UpdateAccessStatusByPollIdAsync(
            poll.Id,
            ideaSource,
            ideaTarget,
            It.IsAny<CancellationToken>()));
        if (failingStep == PollFailingStep.UpdateIdeas)
            ideas.ThrowsAsync(failure!);
        else
            ideas.Callback(() => calls.Add(IdeasStep)).ReturnsAsync(0);

        var transaction = UnitOfWork.SetupTransaction();
        transaction
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add(CommitStep))
            .Returns(Task.CompletedTask);
        transaction
            .Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return transaction;
    }

    [Fact]
    public async Task Handle_PollDoesNotExist_ReturnsNotFoundErrorWithoutTransaction()
    {
        var id = Guid.NewGuid();
        SetupPoll(null, id);

        var result = await _sut.Handle(new ChangePollStatusCommand(id, PollStatus.Active), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.NotFound(id), result.Error);
        UnitOfWork.Mock.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_StatusIsAlreadySet_ReturnsSuccessWithoutChanges()
    {
        var poll = PollFakers.Polls().Generate();
        poll.Status = PollStatus.Active;
        SetupPoll(poll, poll.Id);

        var result = await _sut.Handle(new ChangePollStatusCommand(poll.Id, PollStatus.Active), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(PollStatus.Active, poll.Status);
        UnitOfWork.Mock.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [MemberData(nameof(ChangePollStatusTestData.UnsupportedStatuses), MemberType = typeof(ChangePollStatusTestData))]
    public async Task Handle_StatusIsNotSupported_ReturnsInvalidStatusErrorAndLogsWarning(PollStatus status)
    {
        var poll = PollFakers.Polls().Generate();
        poll.Status = PollStatus.Active;
        SetupPoll(poll, poll.Id);

        var result = await _sut.Handle(new ChangePollStatusCommand(poll.Id, status), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.InvalidStatus(status), result.Error);
        Assert.Equal(PollStatus.Active, poll.Status);
        UnitOfWork.Mock.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [MemberData(nameof(ChangePollStatusTestData.Transitions), MemberType = typeof(ChangePollStatusTestData))]
    public async Task Handle_StatusChanges_UpdatesPollAndIdeasAndCommits(
        PollStatus currentStatus,
        PollStatus newStatus,
        IdeaAccessStatus ideaSource,
        IdeaAccessStatus ideaTarget)
    {
        var poll = PollFakers.Polls().Generate();
        poll.Status = currentStatus;
        var transaction = SetupFlow(poll, ideaSource, ideaTarget);

        var result = await _sut.Handle(new ChangePollStatusCommand(poll.Id, newStatus), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(newStatus, poll.Status);
        UnitOfWork.Ideas.Verify(
            x => x.UpdateAccessStatusByPollIdAsync(poll.Id, ideaSource, ideaTarget, It.IsAny<CancellationToken>()),
            Times.Once);
        transaction.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        transaction.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_StatusChanges_RunsStepsInOrderAndCommitsLast()
    {
        var poll = PollFakers.Polls().Generate();
        poll.Status = PollStatus.Active;
        var calls = new List<string>();
        SetupFlow(poll, IdeaAccessStatus.Active, IdeaAccessStatus.RestrictedByContext, calls);

        await _sut.Handle(new ChangePollStatusCommand(poll.Id, PollStatus.Suspended), CancellationToken.None);

        Assert.Equal([UpdateStep, SaveStep, IdeasStep, CommitStep], calls);
    }

    [Theory]
    [MemberData(nameof(ChangePollStatusTestData.FailingSteps), MemberType = typeof(ChangePollStatusTestData))]
    public async Task Handle_StepFails_RollsBackLogsErrorAndReturnsDatabaseError(PollFailingStep failingStep)
    {
        var poll = PollFakers.Polls().Generate();
        poll.Status = PollStatus.Active;
        var failure = new InvalidOperationException(DatabaseFailureMessage);
        var transaction = SetupFlow(
            poll,
            IdeaAccessStatus.Active,
            IdeaAccessStatus.RestrictedByContext,
            failingStep: failingStep,
            failure: failure);

        var result = await _sut.Handle(new ChangePollStatusCommand(poll.Id, PollStatus.Suspended), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(CommonErrors.DatabaseError, result.Error);
        transaction.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        transaction.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                failure,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
