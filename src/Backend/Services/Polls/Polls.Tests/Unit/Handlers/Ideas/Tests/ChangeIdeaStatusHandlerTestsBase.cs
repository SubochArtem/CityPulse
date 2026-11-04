using Moq;
using Polls.Domain.Common;
using Polls.Domain.Ideas;
using Polls.Domain.Polls;
using Polls.Tests.Infrastructure;
using Polls.Tests.TestData;
using Xunit;
using MediatRUnit = MediatR.Unit;

namespace Polls.Tests.Unit.Handlers.Ideas.Tests;

public abstract class ChangeIdeaStatusHandlerTestsBase<TCommand, TStatus> : HandlerTestsBase
    where TStatus : struct, Enum
{
    private static readonly TStatus[] Statuses = Enum.GetValues<TStatus>();
    private static readonly TStatus UndefinedStatus = default;

    protected abstract TCommand Build(Guid id, TStatus status);

    protected abstract TStatus GetStatus(Idea idea);

    protected abstract void SetStatus(Idea idea, TStatus status);

    protected abstract Error ExpectedInvalidStatusError(TStatus status);

    protected abstract Task<Result<MediatRUnit>> HandleAsync(TCommand command, CancellationToken cancellationToken);

    private void SetupIdea(Idea? idea, Guid id) =>
        UnitOfWork.Ideas
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(idea);

    protected Poll SetupPollFor(Idea idea)
    {
        var poll = PollFakers.Polls().Generate();
        poll.Id = idea.PollId;
        UnitOfWork.Polls
            .Setup(x => x.GetByIdAsync(poll.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(poll);

        return poll;
    }

    protected Idea SetupIdeaWithStatus(TStatus status)
    {
        var idea = IdeaFakers.Ideas().Generate();
        SetStatus(idea, status);
        SetupIdea(idea, idea.Id);
        UnitOfWork.Ideas.Setup(x => x.Update(idea));
        UnitOfWork.SetupSaveChanges();

        return idea;
    }

    protected Task<Result<MediatRUnit>> ChangeStatusAsync(Idea idea, TStatus status) =>
        HandleAsync(Build(idea.Id, status), CancellationToken.None);

    private void VerifyNothingPublished() =>
        EventPublisher.Verify(
            x => x.PublishAsync(It.IsAny<It.IsAnyType>(), It.IsAny<CancellationToken>()),
            Times.Never);

    [Fact]
    public async Task Handle_IdeaDoesNotExist_ReturnsNotFoundErrorAndSavesNothing()
    {
        var id = Guid.NewGuid();
        SetupIdea(null, id);

        var result = await HandleAsync(Build(id, Statuses[0]), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(IdeaErrors.NotFound(id), result.Error);
        UnitOfWork.Ideas.Verify(x => x.Update(It.IsAny<Idea>()), Times.Never);
        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        VerifyNothingPublished();
    }

    [Fact]
    public async Task Handle_StatusIsUndefined_ReturnsInvalidStatusErrorAndSavesNothing()
    {
        foreach (var currentStatus in Statuses)
        {
            var idea = IdeaFakers.Ideas().Generate();
            SetStatus(idea, currentStatus);
            SetupIdea(idea, idea.Id);

            var result = await HandleAsync(Build(idea.Id, UndefinedStatus), CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ExpectedInvalidStatusError(UndefinedStatus), result.Error);
            Assert.Equal(currentStatus, GetStatus(idea));
        }

        UnitOfWork.Ideas.Verify(x => x.Update(It.IsAny<Idea>()), Times.Never);
        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        VerifyNothingPublished();
    }

    [Fact]
    public async Task Handle_StatusIsAlreadySet_ReturnsSuccessAndSavesNothing()
    {
        foreach (var status in Statuses.Where(status => !status.Equals(UndefinedStatus)))
        {
            var idea = IdeaFakers.Ideas().Generate();
            SetStatus(idea, status);
            SetupIdea(idea, idea.Id);

            var result = await HandleAsync(Build(idea.Id, status), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(status, GetStatus(idea));
        }

        UnitOfWork.Ideas.Verify(x => x.Update(It.IsAny<Idea>()), Times.Never);
        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        VerifyNothingPublished();
    }

    [Fact]
    public async Task Handle_StatusIsDifferent_UpdatesStatusAndSavesOnce()
    {
        var currentStatus = Statuses[0];
        var changes = 0;
        UnitOfWork.SetupSaveChanges();

        foreach (var newStatus in Statuses.Skip(1))
        {
            var idea = IdeaFakers.Ideas().Generate();
            SetStatus(idea, currentStatus);
            SetupIdea(idea, idea.Id);
            SetupPollFor(idea);
            UnitOfWork.Ideas.Setup(x => x.Update(idea));

            var result = await HandleAsync(Build(idea.Id, newStatus), CancellationToken.None);
            changes++;

            Assert.True(result.IsSuccess);
            Assert.Equal(newStatus, GetStatus(idea));
            UnitOfWork.Ideas.Verify(x => x.Update(idea), Times.Once);
        }

        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(changes));
        EventPublisher.Verify(
            x => x.PublishAsync(It.IsAny<It.IsAnyType>(), It.IsAny<CancellationToken>()),
            Times.Exactly(changes));
    }

    [Fact]
    public async Task Handle_PollDoesNotExist_ReturnsPollNotFoundAndSavesNothing()
    {
        var idea = IdeaFakers.Ideas().Generate();
        SetStatus(idea, Statuses[0]);
        SetupIdea(idea, idea.Id);
        UnitOfWork.Polls
            .Setup(x => x.GetByIdAsync(idea.PollId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Poll?)null);

        var result = await HandleAsync(Build(idea.Id, Statuses[1]), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.NotFound(idea.PollId), result.Error);
        Assert.Equal(Statuses[0], GetStatus(idea));
        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        VerifyNothingPublished();
    }

    [Fact]
    public async Task Handle_CancellationTokenProvided_PassesItToRepositoryAndSave()
    {
        var idea = IdeaFakers.Ideas().Generate();
        SetStatus(idea, Statuses[0]);
        using var cts = new CancellationTokenSource();
        UnitOfWork.Ideas
            .Setup(x => x.GetByIdAsync(idea.Id, cts.Token))
            .ReturnsAsync(idea);
        SetupPollFor(idea);
        UnitOfWork.Ideas.Setup(x => x.Update(idea));
        UnitOfWork.Mock.Setup(x => x.SaveChangesAsync(cts.Token)).Returns(Task.CompletedTask);

        await HandleAsync(Build(idea.Id, Statuses[1]), cts.Token);

        UnitOfWork.Ideas.Verify(x => x.GetByIdAsync(idea.Id, cts.Token), Times.Once);
        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(cts.Token), Times.Once);
        EventPublisher.Verify(
            x => x.PublishAsync(It.IsAny<It.IsAnyType>(), cts.Token),
            Times.Once);
    }
}
