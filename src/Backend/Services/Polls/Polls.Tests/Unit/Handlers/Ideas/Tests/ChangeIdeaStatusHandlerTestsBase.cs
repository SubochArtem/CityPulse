using Moq;
using Polls.Domain.Common;
using Polls.Domain.Ideas;
using Polls.Tests.Infrastructure;
using Polls.Tests.TestData;
using Xunit;
using MediatRUnit = MediatR.Unit;

namespace Polls.Tests.Unit.Handlers.Ideas.Tests;

public abstract class ChangeIdeaStatusHandlerTestsBase<TCommand, TStatus> : HandlerTestsBase
    where TStatus : struct, Enum
{
    private static readonly TStatus[] Statuses = Enum.GetValues<TStatus>();

    protected abstract TCommand Build(Guid id, TStatus status);

    protected abstract TStatus GetStatus(Idea idea);

    protected abstract void SetStatus(Idea idea, TStatus status);

    protected abstract Task<Result<MediatRUnit>> HandleAsync(TCommand command, CancellationToken cancellationToken);

    private void SetupIdea(Idea? idea, Guid id) =>
        UnitOfWork.Ideas
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(idea);

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
    }

    [Fact]
    public async Task Handle_StatusIsAlreadySet_ReturnsSuccessAndSavesNothing()
    {
        foreach (var status in Statuses)
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
            UnitOfWork.Ideas.Setup(x => x.Update(idea));

            var result = await HandleAsync(Build(idea.Id, newStatus), CancellationToken.None);
            changes++;

            Assert.True(result.IsSuccess);
            Assert.Equal(newStatus, GetStatus(idea));
            UnitOfWork.Ideas.Verify(x => x.Update(idea), Times.Once);
        }

        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(changes));
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
        UnitOfWork.Ideas.Setup(x => x.Update(idea));
        UnitOfWork.Mock.Setup(x => x.SaveChangesAsync(cts.Token)).Returns(Task.CompletedTask);

        await HandleAsync(Build(idea.Id, Statuses[1]), cts.Token);

        UnitOfWork.Ideas.Verify(x => x.GetByIdAsync(idea.Id, cts.Token), Times.Once);
        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(cts.Token), Times.Once);
    }
}
