using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Polls.Application.Common.Models;
using Polls.Application.Ideas.Commands.UpdateIdea;
using Polls.Domain.Ideas;
using Polls.Domain.Images;
using Polls.Tests.Infrastructure;
using Polls.Tests.TestData;
using Polls.Tests.Unit.Handlers.Ideas.TestData;
using Xunit;

namespace Polls.Tests.Unit.Handlers.Ideas.Tests;

public sealed class UpdateIdeaCommandHandlerTests : HandlerTestsBase
{
    private const string NewTitle = "Updated title";
    private const string NewDescription = "Updated description";
    private const string UploadedFileName = "uploaded-new.jpg";

    private readonly UpdateIdeaCommandHandler _sut;

    public UpdateIdeaCommandHandlerTests()
    {
        _sut = new UpdateIdeaCommandHandler(
            UnitOfWork.Object,
            Mapper,
            Storage.Object,
            NullLogger<UpdateIdeaCommandHandler>.Instance);
    }

    private static UpdateIdeaCommand BuildCommand(
        Idea idea,
        Guid? userId = null,
        bool bypassRestrictions = false,
        IReadOnlyList<ImageFile>? imagesToAdd = null,
        IReadOnlyList<Guid>? imagesToDelete = null) =>
        new(
            idea.Id,
            NewTitle,
            NewDescription,
            userId ?? idea.UserId,
            bypassRestrictions,
            imagesToAdd,
            imagesToDelete);

    private void SetupIdea(Idea? idea, Guid id) =>
        UnitOfWork.Ideas
            .Setup(x => x.GetWithPollAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(idea);

    private void SetupSuccessfulUpdate(Idea idea)
    {
        SetupIdea(idea, idea.Id);
        UnitOfWork.Ideas.Setup(x => x.Update(idea));
        UnitOfWork.SetupSaveChanges();
    }

    [Fact]
    public async Task Handle_IdeaDoesNotExist_ReturnsNotFoundErrorAndUpdatesNothing()
    {
        var idea = IdeaFakers.IdeaWithPoll();
        SetupIdea(null, idea.Id);

        var result = await _sut.Handle(BuildCommand(idea), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(IdeaErrors.NotFound(idea.Id), result.Error);
        UnitOfWork.Ideas.Verify(x => x.Update(It.IsAny<Idea>()), Times.Never);
    }

    [Theory]
    [MemberData(nameof(IdeaHandlerTestData.AccessViolations), MemberType = typeof(IdeaHandlerTestData))]
    public async Task Handle_AccessIsRestricted_ReturnsErrorAndUpdatesNothing(IdeaAccessViolationCase violation)
    {
        var idea = IdeaFakers.IdeaWithPoll();
        var originalTitle = idea.Title;
        violation.Prepare(idea);
        SetupIdea(idea, idea.Id);

        var result = await _sut.Handle(BuildCommand(idea, violation.UserIdSelector(idea)), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(violation.ExpectedError(idea), result.Error);
        Assert.Equal(originalTitle, idea.Title);
        UnitOfWork.Ideas.Verify(x => x.Update(It.IsAny<Idea>()), Times.Never);
        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [MemberData(nameof(IdeaHandlerTestData.AccessViolations), MemberType = typeof(IdeaHandlerTestData))]
    public async Task Handle_RestrictionsAreBypassed_UpdatesIdeaDespiteViolation(IdeaAccessViolationCase violation)
    {
        var idea = IdeaFakers.IdeaWithPoll();
        violation.Prepare(idea);
        SetupSuccessfulUpdate(idea);

        var result = await _sut.Handle(
            BuildCommand(idea, violation.UserIdSelector(idea), bypassRestrictions: true),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(NewTitle, idea.Title);
    }

    [Fact]
    public async Task Handle_OwnerUpdatesPendingIdea_UpdatesDetailsAndSaves()
    {
        var idea = IdeaFakers.IdeaWithPoll();
        SetupSuccessfulUpdate(idea);

        var result = await _sut.Handle(BuildCommand(idea), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(NewTitle, idea.Title);
        Assert.Equal(NewDescription, idea.Description);
        UnitOfWork.Ideas.Verify(x => x.Update(idea), Times.Once);
        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_OwnerUpdatesPendingIdea_ReturnsDtoWithUpdatedValues()
    {
        var idea = IdeaFakers.IdeaWithPoll();
        SetupSuccessfulUpdate(idea);

        var result = await _sut.Handle(BuildCommand(idea), CancellationToken.None);

        Assert.Equal(idea.Id, result.Value!.Id);
        Assert.Equal(NewTitle, result.Value.Title);
        Assert.Equal(NewDescription, result.Value.Description);
    }

    [Fact]
    public async Task Handle_ImagesToAddProvided_UploadsThemAndAppendsToIdea()
    {
        var idea = IdeaFakers.IdeaWithImages("existing.jpg");
        var image = ImageFiles.Create("new.jpg", ImageFiles.JpegSignature);
        SetupSuccessfulUpdate(idea);
        Storage
            .Setup(x => x.UploadRangeAsync(It.IsAny<IReadOnlyList<ImageFile>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([UploadedFileName]);

        var result = await _sut.Handle(BuildCommand(idea, imagesToAdd: [image]), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(["existing.jpg", UploadedFileName], idea.Images.Select(i => i.FileName));
        Assert.Equal([0, 1], idea.Images.Select(i => i.Order));
    }

    [Fact]
    public async Task Handle_ImagesToDeleteProvided_RemovesThemAndQueuesFilesForCleanup()
    {
        var idea = IdeaFakers.IdeaWithImages("first.jpg", "second.jpg");
        var imageToDelete = idea.Images.First();
        List<DeletedImage>? queued = null;
        SetupSuccessfulUpdate(idea);
        UnitOfWork.DeletedImages
            .Setup(x => x.AddRange(It.IsAny<IEnumerable<DeletedImage>>()))
            .Callback<IEnumerable<DeletedImage>>(entries => queued = entries.ToList());

        var result = await _sut.Handle(BuildCommand(idea, imagesToDelete: [imageToDelete.Id]), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(imageToDelete, idea.Images);
        Assert.NotNull(queued);
        Assert.Equal(["first.jpg"], queued.Select(entry => entry.FileName));
    }

    [Fact]
    public async Task Handle_ImageToDeleteDoesNotBelongToIdea_ReturnsNotBelongsErrorAndDoesNotSave()
    {
        var idea = IdeaFakers.IdeaWithImages("first.jpg");
        var foreignImageId = Guid.NewGuid();
        SetupIdea(idea, idea.Id);
        UnitOfWork.Ideas.Setup(x => x.Update(idea));

        var result = await _sut.Handle(BuildCommand(idea, imagesToDelete: [foreignImageId]), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ImageErrors.NotBelongsToEntity(foreignImageId), result.Error);
        Assert.Single(idea.Images);
        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
