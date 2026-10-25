using Moq;
using Polls.Application.Common.Interfaces;
using Polls.Application.Polls.Commands.DeletePoll;
using Polls.Domain.Images;
using Polls.Domain.Polls;
using Polls.Tests.Infrastructure;
using Polls.Tests.TestData;
using Polls.Tests.Unit.Handlers.Polls.TestData;
using Xunit;

namespace Polls.Tests.Unit.Handlers.Polls.Tests;

public sealed class DeletePollCommandHandlerTests : HandlerTestsBase
{
    private const string CancelStep = "cancel";
    private const string DeleteStep = "delete";
    private const string SaveStep = "save";

    private readonly Mock<IPollScheduler> _scheduler = new(MockBehavior.Strict);
    private readonly DeletePollCommandHandler _sut;

    public DeletePollCommandHandlerTests()
    {
        _sut = new DeletePollCommandHandler(UnitOfWork.Object, _scheduler.Object);
    }

    private void SetupPoll(Poll? poll, Guid id) =>
        UnitOfWork.Polls
            .Setup(x => x.GetByIdWithImagesAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(poll);

    private void SetupSuccessfulDelete(Poll poll)
    {
        SetupPoll(poll, poll.Id);
        UnitOfWork.Polls.Setup(x => x.Delete(poll));
        UnitOfWork.SetupSaveChanges();
        _scheduler
            .Setup(x => x.CancelAsync(poll.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private void VerifyCancelCalls(Times times) =>
        _scheduler.Verify(x => x.CancelAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), times);

    [Fact]
    public async Task Handle_PollDoesNotExist_ReturnsNotFoundErrorAndDeletesNothing()
    {
        var id = Guid.NewGuid();
        SetupPoll(null, id);

        var result = await _sut.Handle(new DeletePollCommand(id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.NotFound(id), result.Error);
        UnitOfWork.Polls.Verify(x => x.Delete(It.IsAny<Poll>()), Times.Never);
        VerifyCancelCalls(Times.Never());
    }

    [Fact]
    public async Task Handle_UserBelongsToAnotherCity_ReturnsNotFromUserCityErrorAndDeletesNothing()
    {
        var poll = PollFakers.Polls().Generate();
        SetupPoll(poll, poll.Id);

        var result = await _sut.Handle(new DeletePollCommand(poll.Id, Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.NotFromUserCity(poll.CityId), result.Error);
        UnitOfWork.Polls.Verify(x => x.Delete(It.IsAny<Poll>()), Times.Never);
        VerifyCancelCalls(Times.Never());
    }

    [Theory]
    [MemberData(nameof(PollHandlerTestData.Restrictions), MemberType = typeof(PollHandlerTestData))]
    public async Task Handle_PollIsRestricted_ReturnsErrorAndDeletesNothing(PollRestrictionCase restriction)
    {
        var poll = PollFakers.PollWithImages("first.jpg");
        restriction.Prepare(poll);
        SetupPoll(poll, poll.Id);

        var result = await _sut.Handle(new DeletePollCommand(poll.Id, poll.CityId), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(restriction.ExpectedError(poll), result.Error);
        UnitOfWork.Polls.Verify(x => x.Delete(It.IsAny<Poll>()), Times.Never);
        UnitOfWork.DeletedImages.Verify(x => x.AddRange(It.IsAny<IEnumerable<DeletedImage>>()), Times.Never);
        VerifyCancelCalls(Times.Never());
    }

    [Theory]
    [MemberData(nameof(PollHandlerTestData.Restrictions), MemberType = typeof(PollHandlerTestData))]
    public async Task Handle_RestrictionsAreBypassed_DeletesPollDespiteRestriction(PollRestrictionCase restriction)
    {
        var poll = PollFakers.Polls().Generate();
        restriction.Prepare(poll);
        SetupSuccessfulDelete(poll);

        var result = await _sut.Handle(
            new DeletePollCommand(poll.Id, Guid.NewGuid(), BypassRestrictions: true),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        UnitOfWork.Polls.Verify(x => x.Delete(poll), Times.Once);
        VerifyCancelCalls(Times.Once());
    }

    [Fact]
    public async Task Handle_PollIsDeleted_CancelsScheduledJobBeforeDeletingAndSaving()
    {
        var poll = PollFakers.Polls().Generate();
        var calls = new List<string>();
        SetupPoll(poll, poll.Id);
        _scheduler
            .Setup(x => x.CancelAsync(poll.Id, It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add(CancelStep))
            .Returns(Task.CompletedTask);
        UnitOfWork.Polls
            .Setup(x => x.Delete(poll))
            .Callback(() => calls.Add(DeleteStep));
        UnitOfWork.Mock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add(SaveStep))
            .Returns(Task.CompletedTask);

        await _sut.Handle(new DeletePollCommand(poll.Id, poll.CityId), CancellationToken.None);

        Assert.Equal([CancelStep, DeleteStep, SaveStep], calls);
    }

    [Fact]
    public async Task Handle_PollWithoutImages_DeletesAndSavesWithoutQueueingFiles()
    {
        var poll = PollFakers.Polls().Generate();
        SetupSuccessfulDelete(poll);

        var result = await _sut.Handle(new DeletePollCommand(poll.Id, poll.CityId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        UnitOfWork.Polls.Verify(x => x.Delete(poll), Times.Once);
        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        UnitOfWork.DeletedImages.Verify(x => x.AddRange(It.IsAny<IEnumerable<DeletedImage>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_PollWithImages_QueuesImageFilesForCleanup()
    {
        var poll = PollFakers.PollWithImages("first.jpg", "second.png");
        List<DeletedImage>? queued = null;
        SetupSuccessfulDelete(poll);
        UnitOfWork.DeletedImages
            .Setup(x => x.AddRange(It.IsAny<IEnumerable<DeletedImage>>()))
            .Callback<IEnumerable<DeletedImage>>(entries => queued = entries.ToList());

        var result = await _sut.Handle(new DeletePollCommand(poll.Id, poll.CityId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(queued);
        Assert.Equal(["first.jpg", "second.png"], queued.Select(entry => entry.FileName));
        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
