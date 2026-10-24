using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Polls.Application.Common.Constants;
using Polls.Application.Common.Interfaces;
using Polls.Application.Common.Models;
using Polls.Application.Polls.Commands.UpdatePoll;
using Polls.Domain.Images;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.TestData;
using Polls.Tests.Unit.Handlers.TestData;
using Xunit;

namespace Polls.Tests.Unit.Handlers.Tests;

public sealed class UpdatePollCommandHandlerTests : HandlerTestsBase
{
    private const string NewTitle = "Updated title";
    private const string NewDescription = "Updated description";
    private const decimal NewBudget = 2500;
    private const int NewDurationDays = 20;
    private const string UploadedFileName = "uploaded-new.jpg";

    private readonly Mock<IPollScheduler> _scheduler = new(MockBehavior.Strict);
    private readonly UpdatePollCommandHandler _sut;

    public UpdatePollCommandHandlerTests()
    {
        _sut = new UpdatePollCommandHandler(
            UnitOfWork.Object,
            Mapper,
            _scheduler.Object,
            Storage.Object,
            NullLogger<UpdatePollCommandHandler>.Instance);
    }

    private static UpdatePollCommand BuildCommand(
        Poll poll,
        DateTimeOffset? endsAt = null,
        Guid? userCityId = null,
        bool bypassRestrictions = false,
        IReadOnlyList<ImageFile>? imagesToAdd = null,
        IReadOnlyList<Guid>? imagesToDelete = null) =>
        new(
            poll.Id,
            NewTitle,
            NewDescription,
            endsAt ?? DateTimeOffset.UtcNow.AddDays(NewDurationDays),
            NewBudget,
            userCityId ?? poll.CityId,
            bypassRestrictions,
            imagesToAdd,
            imagesToDelete);

    private void SetupPoll(Poll? poll, Guid id) =>
        UnitOfWork.Polls
            .Setup(x => x.GetByIdWithImagesAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(poll);

    private void SetupSuccessfulUpdate(Poll poll, bool expectSchedule = true)
    {
        SetupPoll(poll, poll.Id);
        UnitOfWork.Polls.Setup(x => x.Update(poll));
        UnitOfWork.SetupSaveChanges();

        if (expectSchedule)
        {
            _scheduler
                .Setup(x => x.ScheduleAsync(poll.Id, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }
    }

    private void VerifyNotScheduled() =>
        _scheduler.Verify(
            x => x.ScheduleAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()),
            Times.Never);

    [Fact]
    public async Task Handle_PollDoesNotExist_ReturnsNotFoundErrorAndUpdatesNothing()
    {
        var poll = PollFakers.Polls().Generate();
        SetupPoll(null, poll.Id);

        var result = await _sut.Handle(BuildCommand(poll), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.NotFound(poll.Id), result.Error);
        UnitOfWork.Polls.Verify(x => x.Update(It.IsAny<Poll>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UserBelongsToAnotherCity_ReturnsNotFromUserCityErrorAndUpdatesNothing()
    {
        var poll = PollFakers.Polls().Generate();
        SetupPoll(poll, poll.Id);

        var result = await _sut.Handle(BuildCommand(poll, userCityId: Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.NotFromUserCity(poll.CityId), result.Error);
        UnitOfWork.Polls.Verify(x => x.Update(It.IsAny<Poll>()), Times.Never);
    }

    [Theory]
    [MemberData(nameof(PollHandlerTestData.Restrictions), MemberType = typeof(PollHandlerTestData))]
    public async Task Handle_PollIsRestricted_ReturnsErrorAndUpdatesNothing(PollRestrictionCase restriction)
    {
        var poll = PollFakers.Polls().Generate();
        var originalTitle = poll.Title;
        restriction.Prepare(poll);
        SetupPoll(poll, poll.Id);

        var result = await _sut.Handle(BuildCommand(poll), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(restriction.ExpectedError(poll), result.Error);
        Assert.Equal(originalTitle, poll.Title);
        UnitOfWork.Polls.Verify(x => x.Update(It.IsAny<Poll>()), Times.Never);
        VerifyNotScheduled();
    }

    [Fact]
    public async Task Handle_NewEndDateMakesPollTooLong_ReturnsMaxDurationExceededError()
    {
        var poll = PollFakers.Polls().Generate();
        SetupPoll(poll, poll.Id);
        var tooFar = poll.CreatedAt.AddDays(ValidationConstants.Poll.MaxDurationDays + 1);

        var result = await _sut.Handle(BuildCommand(poll, endsAt: tooFar), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.MaxDurationExceeded(ValidationConstants.Poll.MaxDurationDays), result.Error);
        UnitOfWork.Polls.Verify(x => x.Update(It.IsAny<Poll>()), Times.Never);
    }

    [Theory]
    [MemberData(nameof(PollHandlerTestData.Restrictions), MemberType = typeof(PollHandlerTestData))]
    public async Task Handle_RestrictionsAreBypassed_UpdatesPollDespiteRestriction(PollRestrictionCase restriction)
    {
        var poll = PollFakers.Polls().Generate();
        restriction.Prepare(poll);
        SetupSuccessfulUpdate(poll, expectSchedule: poll.Status == PollStatus.Active);

        var result = await _sut.Handle(BuildCommand(poll, bypassRestrictions: true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(NewTitle, poll.Title);
    }

    [Fact]
    public async Task Handle_PollIsEditable_UpdatesDetailsAndSaves()
    {
        var poll = PollFakers.Polls().Generate();
        var newEndsAt = DateTimeOffset.UtcNow.AddDays(NewDurationDays);
        SetupSuccessfulUpdate(poll);

        var result = await _sut.Handle(BuildCommand(poll, endsAt: newEndsAt), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(NewTitle, poll.Title);
        Assert.Equal(NewDescription, poll.Description);
        Assert.Equal(newEndsAt, poll.EndsAt);
        Assert.Equal(NewBudget, poll.BudgetAmount);
        UnitOfWork.Polls.Verify(x => x.Update(poll), Times.Once);
        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PollIsEditable_ReturnsDtoWithUpdatedValues()
    {
        var poll = PollFakers.Polls().Generate();
        SetupSuccessfulUpdate(poll);

        var result = await _sut.Handle(BuildCommand(poll), CancellationToken.None);

        Assert.Equal(poll.Id, result.Value!.Id);
        Assert.Equal(NewTitle, result.Value.Title);
        Assert.Equal(NewDescription, result.Value.Description);
        Assert.Equal(NewBudget, result.Value.BudgetAmount);
    }

    [Fact]
    public async Task Handle_EndDateChangedForActivePoll_ReschedulesPollEnd()
    {
        var poll = PollFakers.Polls().Generate();
        var newEndsAt = DateTimeOffset.UtcNow.AddDays(NewDurationDays);
        SetupSuccessfulUpdate(poll);

        await _sut.Handle(BuildCommand(poll, endsAt: newEndsAt), CancellationToken.None);

        _scheduler.Verify(
            x => x.ScheduleAsync(poll.Id, newEndsAt, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_EndDateIsUnchanged_DoesNotReschedule()
    {
        var poll = PollFakers.Polls().Generate();
        SetupSuccessfulUpdate(poll, expectSchedule: false);

        var result = await _sut.Handle(BuildCommand(poll, endsAt: poll.EndsAt), CancellationToken.None);

        Assert.True(result.IsSuccess);
        VerifyNotScheduled();
    }

    [Fact]
    public async Task Handle_EndDateChangedForSuspendedPoll_DoesNotReschedule()
    {
        var poll = PollFakers.Polls().Generate();
        poll.Status = PollStatus.Suspended;
        SetupSuccessfulUpdate(poll, expectSchedule: false);

        var result = await _sut.Handle(BuildCommand(poll, bypassRestrictions: true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        VerifyNotScheduled();
    }

    [Fact]
    public async Task Handle_ImagesToAddProvided_UploadsThemAndAppendsToPoll()
    {
        var poll = PollFakers.PollWithImages("existing.jpg");
        var image = ImageFiles.Create("new.jpg", ImageFiles.JpegSignature);
        SetupSuccessfulUpdate(poll);
        Storage
            .Setup(x => x.UploadRangeAsync(It.IsAny<IReadOnlyList<ImageFile>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([UploadedFileName]);

        var result = await _sut.Handle(BuildCommand(poll, imagesToAdd: [image]), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(["existing.jpg", UploadedFileName], poll.Images.Select(i => i.FileName));
        Assert.Equal([0, 1], poll.Images.Select(i => i.Order));
    }

    [Fact]
    public async Task Handle_ImagesToDeleteProvided_RemovesThemAndQueuesFilesForCleanup()
    {
        var poll = PollFakers.PollWithImages("first.jpg", "second.jpg");
        var imageToDelete = poll.Images.First();
        List<DeletedImage>? queued = null;
        SetupSuccessfulUpdate(poll);
        UnitOfWork.DeletedImages
            .Setup(x => x.AddRange(It.IsAny<IEnumerable<DeletedImage>>()))
            .Callback<IEnumerable<DeletedImage>>(entries => queued = entries.ToList());

        var result = await _sut.Handle(BuildCommand(poll, imagesToDelete: [imageToDelete.Id]), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(imageToDelete, poll.Images);
        Assert.NotNull(queued);
        Assert.Equal(["first.jpg"], queued.Select(entry => entry.FileName));
    }

    [Fact]
    public async Task Handle_ImageToDeleteDoesNotBelongToPoll_ReturnsNotBelongsErrorAndDoesNotSaveOrSchedule()
    {
        var poll = PollFakers.PollWithImages("first.jpg");
        var foreignImageId = Guid.NewGuid();
        SetupPoll(poll, poll.Id);
        UnitOfWork.Polls.Setup(x => x.Update(poll));

        var result = await _sut.Handle(BuildCommand(poll, imagesToDelete: [foreignImageId]), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ImageErrors.NotBelongsToEntity(foreignImageId), result.Error);
        Assert.Single(poll.Images);
        UnitOfWork.Mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        VerifyNotScheduled();
    }
}
