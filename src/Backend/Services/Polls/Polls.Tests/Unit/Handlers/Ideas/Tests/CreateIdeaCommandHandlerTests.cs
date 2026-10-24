using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Polls.Application.Common.Constants;
using Polls.Application.Common.Models;
using Polls.Application.Ideas.Commands.CreateIdea;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Images;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.TestData;
using Polls.Tests.Unit.Guards.TestData;
using Xunit;

namespace Polls.Tests.Unit.Handlers.Ideas.Tests;

public sealed class CreateIdeaCommandHandlerTests : HandlerTestsBase
{
    private const string Title = "Park renovation";
    private const string Description = "Plant more trees in the central park";
    private const string UploadedFileName = "uploaded-a.jpg";

    private readonly Guid _userId = Guid.NewGuid();
    private readonly CreateIdeaCommandHandler _sut;

    public CreateIdeaCommandHandlerTests()
    {
        _sut = new CreateIdeaCommandHandler(
            UnitOfWork.Object,
            Mapper,
            Storage.Object,
            NullLogger<CreateIdeaCommandHandler>.Instance);
    }

    private CreateIdeaCommand BuildCommand(
        Poll poll,
        Guid? userCityId = null,
        bool bypassRestrictions = false,
        IReadOnlyList<ImageFile>? images = null) =>
        new(
            _userId,
            poll.Id,
            Title,
            Description,
            userCityId ?? poll.CityId,
            bypassRestrictions,
            images);

    private void SetupPoll(Poll? poll, Guid pollId) =>
        UnitOfWork.Polls
            .Setup(x => x.GetByIdAsync(pollId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(poll);

    private Poll SetupExistingPoll()
    {
        var poll = PollFakers.Polls().Generate();
        SetupPoll(poll, poll.Id);

        return poll;
    }

    [Fact]
    public async Task Handle_PollDoesNotExist_ReturnsNotFoundErrorAndCreatesNothing()
    {
        var poll = PollFakers.Polls().Generate();
        SetupPoll(null, poll.Id);

        var result = await _sut.Handle(BuildCommand(poll), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.NotFound(poll.Id), result.Error);
        UnitOfWork.Ideas.Verify(x => x.Create(It.IsAny<Idea>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UserBelongsToAnotherCity_ReturnsNotFromUserCityError()
    {
        var poll = SetupExistingPoll();

        var result = await _sut.Handle(BuildCommand(poll, userCityId: Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.NotFromUserCity(poll.CityId), result.Error);
        UnitOfWork.Ideas.Verify(x => x.Create(It.IsAny<Idea>()), Times.Never);
    }

    [Theory]
    [MemberData(nameof(PollGuardTestData.NotActiveStatuses), MemberType = typeof(PollGuardTestData))]
    public async Task Handle_PollIsNotActive_ReturnsAlreadyFinishedError(PollStatus status)
    {
        var poll = SetupExistingPoll();
        poll.Status = status;

        var result = await _sut.Handle(BuildCommand(poll), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.AlreadyFinished(poll.Id), result.Error);
        UnitOfWork.Ideas.Verify(x => x.Create(It.IsAny<Idea>()), Times.Never);
    }

    [Fact]
    public async Task Handle_PollHasEnded_ReturnsAlreadyFinishedError()
    {
        var poll = SetupExistingPoll();
        poll.EndsAt = DateTimeOffset.UtcNow.AddMinutes(-1);

        var result = await _sut.Handle(BuildCommand(poll), CancellationToken.None);

        Assert.Equal(PollErrors.AlreadyFinished(poll.Id), result.Error);
    }

    [Fact]
    public async Task Handle_CityAndPollStatusAreBothInvalid_ReturnsCityError()
    {
        var poll = SetupExistingPoll();
        poll.Status = PollStatus.Finished;

        var result = await _sut.Handle(BuildCommand(poll, userCityId: Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(PollErrors.NotFromUserCity(poll.CityId), result.Error);
    }

    [Fact]
    public async Task Handle_RestrictionsAreBypassed_CreatesIdeaEvenForOtherCityAndFinishedPoll()
    {
        var poll = SetupExistingPoll();
        poll.Status = PollStatus.Finished;
        UnitOfWork.Ideas.Setup(x => x.Create(It.IsAny<Idea>()));
        UnitOfWork.SetupSaveChanges();

        var result = await _sut.Handle(
            BuildCommand(poll, userCityId: Guid.NewGuid(), bypassRestrictions: true),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        UnitOfWork.Ideas.Verify(x => x.Create(It.IsAny<Idea>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PollIsOpen_CreatesPendingActiveIdeaWithGivenDetailsAndSaves()
    {
        Idea? created = null;
        var poll = SetupExistingPoll();
        UnitOfWork.Ideas
            .Setup(x => x.Create(It.IsAny<Idea>()))
            .Callback<Idea>(idea => created = idea);
        UnitOfWork.SetupSaveChanges();

        var result = await _sut.Handle(BuildCommand(poll), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(created);
        Assert.Equal(_userId, created.UserId);
        Assert.Equal(poll.Id, created.PollId);
        Assert.Equal(Title, created.Title);
        Assert.Equal(Description, created.Description);
        Assert.Equal(IdeaAccessStatus.Active, created.AccessStatus);
        Assert.Equal(IdeaApprovalStatus.Pending, created.ApprovalStatus);
        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PollIsOpen_ReturnsDtoOfCreatedIdea()
    {
        var poll = SetupExistingPoll();
        UnitOfWork.Ideas.Setup(x => x.Create(It.IsAny<Idea>()));
        UnitOfWork.SetupSaveChanges();

        var result = await _sut.Handle(BuildCommand(poll), CancellationToken.None);

        Assert.Equal(_userId, result.Value!.UserId);
        Assert.Equal(poll.Id, result.Value.PollId);
        Assert.Equal(Title, result.Value.Title);
        Assert.Equal((int)IdeaAccessStatus.Active, result.Value.AccessStatus);
        Assert.Equal((int)IdeaApprovalStatus.Pending, result.Value.ApprovalStatus);
        Assert.Empty(result.Value.Images);
    }

    [Fact]
    public async Task Handle_ImagesProvided_UploadsThemAndAttachesToIdea()
    {
        Idea? created = null;
        var poll = SetupExistingPoll();
        var image = ImageFiles.Create("a.jpg", ImageFiles.JpegSignature);
        UnitOfWork.Ideas
            .Setup(x => x.Create(It.IsAny<Idea>()))
            .Callback<Idea>(idea => created = idea);
        UnitOfWork.SetupSaveChanges();
        Storage
            .Setup(x => x.UploadRangeAsync(It.IsAny<IReadOnlyList<ImageFile>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([UploadedFileName]);

        var result = await _sut.Handle(BuildCommand(poll, images: [image]), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var attached = Assert.Single(created!.Images);
        Assert.Equal(UploadedFileName, attached.FileName);
        var dtoImage = Assert.Single(result.Value!.Images);
        Assert.Equal(StorageUrlPrefix + UploadedFileName, dtoImage.Url);
    }

    [Fact]
    public async Task Handle_TooManyImages_ReturnsErrorAndDoesNotUploadOrSave()
    {
        var poll = SetupExistingPoll();
        var images = Enumerable
            .Range(0, ValidationConstants.Image.MaxCount + 1)
            .Select(index => ImageFiles.Create($"image-{index}.jpg", ImageFiles.JpegSignature))
            .ToList();
        UnitOfWork.Ideas.Setup(x => x.Create(It.IsAny<Idea>()));

        var result = await _sut.Handle(BuildCommand(poll, images: images), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ImageErrors.MaxImagesExceeded(ValidationConstants.Image.MaxCount), result.Error);
        Storage.Verify(
            x => x.UploadRangeAsync(It.IsAny<IReadOnlyList<ImageFile>>(), It.IsAny<CancellationToken>()),
            Times.Never);
        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
