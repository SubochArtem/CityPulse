using CityPulse.Contracts.Querying.Pagination;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Polls.Application.Common.Constants;
using Polls.Application.Common.Interfaces;
using Polls.Application.Common.Models;
using Polls.Application.Polls.Commands.CreatePoll;
using Polls.Domain.Cities;
using Polls.Domain.Images;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.TestData;
using Polls.Tests.Unit.Polls.TestData;
using Xunit;

namespace Polls.Tests.Unit.Polls.Tests;

public sealed class CreatePollCommandHandlerTests : HandlerTestsBase
{
    private const string Title = "Budget for the new playground";
    private const string Description = "Choose how to spend the city budget";
    private const decimal Budget = 1000;
    private const string UploadedFileName = "uploaded-a.jpg";
    private const string CreateStep = "create";
    private const string SaveStep = "save";
    private const string ScheduleStep = "schedule";

    private static readonly DateTimeOffset EndsAt = DateTimeOffset.UtcNow.AddDays(10);

    private readonly City _city = CityFakers.Cities().Generate();
    private readonly Mock<IPollScheduler> _scheduler = new(MockBehavior.Strict);
    private readonly CreatePollCommandHandler _sut;

    public CreatePollCommandHandlerTests()
    {
        _sut = new CreatePollCommandHandler(
            UnitOfWork.Object,
            Mapper,
            _scheduler.Object,
            Storage.Object,
            NullLogger<CreatePollCommandHandler>.Instance);
    }

    private CreatePollCommand BuildCommand(
        Guid? userCityId = null,
        bool bypassRestrictions = false,
        PollType type = PollType.UserVote,
        IReadOnlyList<ImageFile>? images = null) =>
        new(
            _city.Id,
            Title,
            Description,
            type,
            EndsAt,
            Budget,
            userCityId ?? _city.Id,
            bypassRestrictions,
            images);

    private void SetupCity(City? city) =>
        UnitOfWork.Cities
            .Setup(x => x.GetByIdAsync(_city.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

    private void SetupActivePolls(int totalCount, Action<PollFilter>? onFilter = null) =>
        UnitOfWork.Polls
            .Setup(x => x.GetFilteredAsync(It.IsAny<PollFilter>(), It.IsAny<CancellationToken>()))
            .Callback<PollFilter, CancellationToken>((filter, _) => onFilter?.Invoke(filter))
            .ReturnsAsync(new PagedList<Poll>([], 1, 10, totalCount));

    private void SetupScheduler() =>
        _scheduler
            .Setup(x => x.ScheduleAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

    private void SetupSuccessfulCreate(Action<Poll>? onCreate = null)
    {
        SetupCity(_city);
        SetupActivePolls(0);
        UnitOfWork.Polls
            .Setup(x => x.Create(It.IsAny<Poll>()))
            .Callback<Poll>(poll => onCreate?.Invoke(poll));
        UnitOfWork.SetupSaveChanges();
        SetupScheduler();
    }

    [Fact]
    public async Task Handle_UserBelongsToAnotherCity_ReturnsNotFromUserCityErrorBeforeLookingUpCity()
    {
        var result = await _sut.Handle(BuildCommand(userCityId: Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.NotFromUserCity(_city.Id), result.Error);
        UnitOfWork.Cities.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_RestrictionsAreBypassed_CreatesPollForAnotherCityUser()
    {
        SetupSuccessfulCreate();

        var result = await _sut.Handle(
            BuildCommand(userCityId: Guid.NewGuid(), bypassRestrictions: true),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        UnitOfWork.Polls.Verify(x => x.Create(It.IsAny<Poll>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CityDoesNotExist_ReturnsNotFoundError()
    {
        SetupCity(null);

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(CityErrors.NotFound(_city.Id), result.Error);
        UnitOfWork.Polls.Verify(x => x.Create(It.IsAny<Poll>()), Times.Never);
    }

    [Theory]
    [MemberData(nameof(PollHandlerTestData.PollTypes), MemberType = typeof(PollHandlerTestData))]
    public async Task Handle_ActivePollOfSameTypeExists_ReturnsAlreadyExistsErrorAndChecksByCityTypeAndStatus(
        PollType type)
    {
        PollFilter? usedFilter = null;
        SetupCity(_city);
        SetupActivePolls(1, filter => usedFilter = filter);

        var result = await _sut.Handle(BuildCommand(type: type), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.AlreadyExists(_city.Id), result.Error);
        Assert.NotNull(usedFilter);
        Assert.Equal(_city.Id, usedFilter.CityId);
        Assert.Equal(type, usedFilter.Type);
        Assert.Equal(PollStatus.Active, usedFilter.Status);
        UnitOfWork.Polls.Verify(x => x.Create(It.IsAny<Poll>()), Times.Never);
    }

    [Theory]
    [MemberData(nameof(PollHandlerTestData.PollTypes), MemberType = typeof(PollHandlerTestData))]
    public async Task Handle_NoActivePoll_CreatesActivePollWithGivenDetailsAndSaves(PollType type)
    {
        Poll? created = null;
        SetupSuccessfulCreate(poll => created = poll);

        var result = await _sut.Handle(BuildCommand(type: type), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(created);
        Assert.Equal(_city.Id, created.CityId);
        Assert.Equal(Title, created.Title);
        Assert.Equal(Description, created.Description);
        Assert.Equal(EndsAt, created.EndsAt);
        Assert.Equal(type, created.Type);
        Assert.Equal(Budget, created.BudgetAmount);
        Assert.Equal(PollStatus.Active, created.Status);
        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoActivePoll_ReturnsDtoOfCreatedPoll()
    {
        SetupSuccessfulCreate();

        var result = await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.Equal(_city.Id, result.Value!.CityId);
        Assert.Equal(Title, result.Value.Title);
        Assert.Equal(Budget, result.Value.BudgetAmount);
        Assert.Equal((int)PollStatus.Active, result.Value.Status);
        Assert.Empty(result.Value.Images);
    }

    [Fact]
    public async Task Handle_PollCreated_SchedulesItsEndAfterSaving()
    {
        var calls = new List<string>();
        Poll? created = null;
        SetupCity(_city);
        SetupActivePolls(0);
        UnitOfWork.Polls
            .Setup(x => x.Create(It.IsAny<Poll>()))
            .Callback<Poll>(poll =>
            {
                created = poll;
                calls.Add(CreateStep);
            });
        UnitOfWork.Mock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add(SaveStep))
            .Returns(Task.CompletedTask);
        _scheduler
            .Setup(x => x.ScheduleAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add(ScheduleStep))
            .Returns(Task.CompletedTask);

        await _sut.Handle(BuildCommand(), CancellationToken.None);

        Assert.Equal([CreateStep, SaveStep, ScheduleStep], calls);
        _scheduler.Verify(
            x => x.ScheduleAsync(created!.Id, EndsAt, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ImagesProvided_UploadsThemAndAttachesToPoll()
    {
        Poll? created = null;
        var image = ImageFiles.Create("a.jpg", ImageFiles.JpegSignature);
        SetupSuccessfulCreate(poll => created = poll);
        Storage
            .Setup(x => x.UploadRangeAsync(It.IsAny<IReadOnlyList<ImageFile>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([UploadedFileName]);

        var result = await _sut.Handle(BuildCommand(images: [image]), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var attached = Assert.Single(created!.Images);
        Assert.Equal(UploadedFileName, attached.FileName);
        var dtoImage = Assert.Single(result.Value!.Images);
        Assert.Equal(StorageUrlPrefix + UploadedFileName, dtoImage.Url);
    }

    [Fact]
    public async Task Handle_TooManyImages_ReturnsErrorAndDoesNotSaveOrSchedule()
    {
        var images = Enumerable
            .Range(0, ValidationConstants.Image.MaxCount + 1)
            .Select(index => ImageFiles.Create($"image-{index}.jpg", ImageFiles.JpegSignature))
            .ToList();
        SetupCity(_city);
        SetupActivePolls(0);
        UnitOfWork.Polls.Setup(x => x.Create(It.IsAny<Poll>()));

        var result = await _sut.Handle(BuildCommand(images: images), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ImageErrors.MaxImagesExceeded(ValidationConstants.Image.MaxCount), result.Error);
        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _scheduler.Verify(
            x => x.ScheduleAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
