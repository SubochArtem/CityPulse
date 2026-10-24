using Moq;
using Polls.Application.Ideas.Commands.DeleteIdea;
using Polls.Domain.Ideas;
using Polls.Domain.Images;
using Polls.Tests.Infrastructure;
using Polls.Tests.TestData;
using Polls.Tests.Unit.Handlers.TestData;
using Xunit;

namespace Polls.Tests.Unit.Handlers.Tests;

public sealed class DeleteIdeaCommandHandlerTests : HandlerTestsBase
{
    private readonly DeleteIdeaCommandHandler _sut;

    public DeleteIdeaCommandHandlerTests()
    {
        _sut = new DeleteIdeaCommandHandler(UnitOfWork.Object);
    }

    private void SetupIdea(Idea? idea, Guid id) =>
        UnitOfWork.Ideas
            .Setup(x => x.GetWithPollAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(idea);

    [Fact]
    public async Task Handle_IdeaDoesNotExist_ReturnsNotFoundErrorAndDeletesNothing()
    {
        var id = Guid.NewGuid();
        SetupIdea(null, id);

        var result = await _sut.Handle(new DeleteIdeaCommand(id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(IdeaErrors.NotFound(id), result.Error);
        UnitOfWork.Ideas.Verify(x => x.Delete(It.IsAny<Idea>()), Times.Never);
    }

    [Theory]
    [MemberData(nameof(IdeaHandlerTestData.AccessViolations), MemberType = typeof(IdeaHandlerTestData))]
    public async Task Handle_AccessIsRestricted_ReturnsErrorAndDeletesNothing(IdeaAccessViolationCase violation)
    {
        var idea = IdeaFakers.IdeaWithImages("first.jpg");
        violation.Prepare(idea);
        SetupIdea(idea, idea.Id);

        var result = await _sut.Handle(
            new DeleteIdeaCommand(idea.Id, violation.UserIdSelector(idea)),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(violation.ExpectedError(idea), result.Error);
        UnitOfWork.Ideas.Verify(x => x.Delete(It.IsAny<Idea>()), Times.Never);
        UnitOfWork.DeletedImages.Verify(x => x.AddRange(It.IsAny<IEnumerable<DeletedImage>>()), Times.Never);
    }

    [Theory]
    [MemberData(nameof(IdeaHandlerTestData.AccessViolations), MemberType = typeof(IdeaHandlerTestData))]
    public async Task Handle_RestrictionsAreBypassed_DeletesIdeaDespiteViolation(IdeaAccessViolationCase violation)
    {
        var idea = IdeaFakers.IdeaWithPoll();
        violation.Prepare(idea);
        SetupIdea(idea, idea.Id);
        UnitOfWork.Ideas.Setup(x => x.Delete(idea));
        UnitOfWork.SetupSaveChanges();

        var result = await _sut.Handle(
            new DeleteIdeaCommand(idea.Id, violation.UserIdSelector(idea), BypassRestrictions: true),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        UnitOfWork.Ideas.Verify(x => x.Delete(idea), Times.Once);
    }

    [Fact]
    public async Task Handle_OwnerDeletesIdeaWithoutImages_DeletesAndSavesWithoutQueueingFiles()
    {
        var idea = IdeaFakers.IdeaWithPoll();
        SetupIdea(idea, idea.Id);
        UnitOfWork.Ideas.Setup(x => x.Delete(idea));
        UnitOfWork.SetupSaveChanges();

        var result = await _sut.Handle(new DeleteIdeaCommand(idea.Id, idea.UserId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        UnitOfWork.Ideas.Verify(x => x.Delete(idea), Times.Once);
        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        UnitOfWork.DeletedImages.Verify(x => x.AddRange(It.IsAny<IEnumerable<DeletedImage>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_OwnerDeletesIdeaWithImages_QueuesImageFilesForCleanup()
    {
        var idea = IdeaFakers.IdeaWithImages("first.jpg", "second.png");
        List<DeletedImage>? queued = null;
        SetupIdea(idea, idea.Id);
        UnitOfWork.DeletedImages
            .Setup(x => x.AddRange(It.IsAny<IEnumerable<DeletedImage>>()))
            .Callback<IEnumerable<DeletedImage>>(entries => queued = entries.ToList());
        UnitOfWork.Ideas.Setup(x => x.Delete(idea));
        UnitOfWork.SetupSaveChanges();

        var result = await _sut.Handle(new DeleteIdeaCommand(idea.Id, idea.UserId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(queued);
        Assert.Equal(["first.jpg", "second.png"], queued.Select(entry => entry.FileName));
        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
