using Microsoft.Extensions.Logging;
using Moq;
using Polls.Application.Common.Constants;
using Polls.Application.Common.Interfaces;
using Polls.Application.Common.Models;
using Polls.Application.Images.Helpers;
using Polls.Domain.Common;
using Polls.Domain.Images;
using Polls.Tests.TestData;
using Xunit;
using MediatRUnit = MediatR.Unit;

namespace Polls.Tests.Unit.Images.Tests;

public sealed class ImageProcessingHelperTests
{
    private const string UploadFailureMessage = "storage unavailable";
    private const string FactoryFailureMessage = "factory failed";

    private static readonly ImageFactory<PollImage> Factory =
        (fileName, order) => new PollImage { FileName = fileName, Order = order };

    private readonly Mock<IUnitOfWork> _unitOfWork = new(MockBehavior.Strict);
    private readonly Mock<IDeletedImageRepository> _deletedImages = new(MockBehavior.Strict);
    private readonly Mock<IImageStorageService> _storage = new(MockBehavior.Strict);
    private readonly Mock<ILogger> _logger = new();

    public ImageProcessingHelperTests()
    {
        _unitOfWork.Setup(x => x.DeletedImages).Returns(_deletedImages.Object);
    }

    private static List<PollImage> BuildImages(params int[] orders) =>
        orders
            .Select(order => new PollImage
            {
                Id = Guid.NewGuid(),
                FileName = $"existing-{order}.jpg",
                Order = order
            })
            .ToList();

    private static List<ImageFile> BuildFiles(int count) =>
        Enumerable
            .Range(0, count)
            .Select(index => ImageFiles.Create($"new-{index}.jpg", ImageFiles.JpegSignature))
            .ToList();

    private static string[] UploadedNames(int count) =>
        Enumerable.Range(0, count).Select(index => $"uploaded-{index}.jpg").ToArray();

    private void SetupUpload(string[] uploadedNames) =>
        _storage
            .Setup(x => x.UploadRangeAsync(It.IsAny<IReadOnlyList<ImageFile>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(uploadedNames);

    private void SetupDeletedImages() =>
        _deletedImages.Setup(x => x.AddRange(It.IsAny<IEnumerable<DeletedImage>>()));

    private Task<Result<MediatRUnit>> ProcessAsync(
        List<PollImage> images,
        ImageChanges changes,
        CancellationToken cancellationToken = default,
        ImageFactory<PollImage>? factory = null) =>
        ImageProcessingHelper.ProcessChangesAsync(
            images,
            _unitOfWork.Object,
            _storage.Object,
            _logger.Object,
            factory ?? Factory,
            changes,
            cancellationToken);

    [Fact]
    public async Task ProcessChangesAsync_NoChanges_ReturnsSuccessAndCallsNothing()
    {
        var images = BuildImages(0, 1);

        var result = await ProcessAsync(images, new ImageChanges());

        Assert.True(result.IsSuccess);
        Assert.Equal(2, images.Count);
    }

    [Fact]
    public async Task ProcessChangesAsync_EmptyLists_ReturnsSuccessAndCallsNothing()
    {
        var images = BuildImages(0);

        var result = await ProcessAsync(images, new ImageChanges([], []));

        Assert.True(result.IsSuccess);
        Assert.Single(images);
    }

    [Fact]
    public async Task ProcessChangesAsync_AddingToEmptyCollection_UploadsFilesAndNumbersImagesFromZero()
    {
        var images = new List<PollImage>();
        var files = BuildFiles(2);
        SetupUpload(UploadedNames(2));

        var result = await ProcessAsync(images, new ImageChanges(files));

        Assert.True(result.IsSuccess);
        Assert.Equal(["uploaded-0.jpg", "uploaded-1.jpg"], images.Select(i => i.FileName));
        Assert.Equal([0, 1], images.Select(i => i.Order));
    }

    [Fact]
    public async Task ProcessChangesAsync_AddingToExistingImages_ContinuesNumberingAfterMaxOrder()
    {
        var images = BuildImages(0, 3);
        SetupUpload(UploadedNames(2));

        var result = await ProcessAsync(images, new ImageChanges(BuildFiles(2)));

        Assert.True(result.IsSuccess);
        Assert.Equal([0, 3, 4, 5], images.Select(i => i.Order));
    }

    [Fact]
    public async Task ProcessChangesAsync_Adding_PassesFilesAndCancellationTokenToStorage()
    {
        var files = BuildFiles(1);
        using var cts = new CancellationTokenSource();
        SetupUpload(UploadedNames(1));

        await ProcessAsync([], new ImageChanges(files), cts.Token);

        _storage.Verify(x => x.UploadRangeAsync(files, cts.Token), Times.Once);
    }

    [Fact]
    public async Task ProcessChangesAsync_TooManyFilesToAdd_ReturnsErrorAndDoesNotUploadOrDelete()
    {
        var images = BuildImages(0, 1);
        var idToDelete = images[0].Id;
        var changes = new ImageChanges(BuildFiles(ValidationConstants.Image.MaxCount + 1), [idToDelete]);

        var result = await ProcessAsync(images, changes);

        Assert.False(result.IsSuccess);
        Assert.Equal(ImageErrors.MaxImagesExceeded(ValidationConstants.Image.MaxCount), result.Error);
        Assert.Equal(2, images.Count);
        _storage.VerifyNoOtherCalls();
        _deletedImages.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProcessChangesAsync_DeletedImagesFreeUpSlots_AllowsAddingUpToLimit()
    {
        var images = BuildImages(0, 1, 2, 3, 4);
        var idsToDelete = new[] { images[0].Id, images[1].Id };
        SetupDeletedImages();
        SetupUpload(UploadedNames(2));

        var result = await ProcessAsync(images, new ImageChanges(BuildFiles(2), idsToDelete));

        Assert.True(result.IsSuccess);
        Assert.Equal(ValidationConstants.Image.MaxCount, images.Count);
    }

    [Fact]
    public async Task ProcessChangesAsync_CollectionIsFullAndNothingDeleted_ReturnsMaxImagesExceededError()
    {
        var images = BuildImages(0, 1, 2, 3, 4);

        var result = await ProcessAsync(images, new ImageChanges(BuildFiles(1)));

        Assert.False(result.IsSuccess);
        Assert.Equal(ImageErrors.MaxImagesExceeded(ValidationConstants.Image.MaxCount), result.Error);
    }

    [Fact]
    public async Task ProcessChangesAsync_Deleting_RemovesImagesAndQueuesTheirFilesForCleanup()
    {
        var images = BuildImages(0, 1, 2);
        var toDelete = images[1];
        List<DeletedImage>? queued = null;
        _deletedImages
            .Setup(x => x.AddRange(It.IsAny<IEnumerable<DeletedImage>>()))
            .Callback<IEnumerable<DeletedImage>>(entries => queued = entries.ToList());

        var result = await ProcessAsync(images, new ImageChanges(ToDeleteIds: [toDelete.Id]));

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(toDelete, images);
        Assert.Equal(2, images.Count);
        Assert.NotNull(queued);
        Assert.Equal([toDelete.FileName], queued.Select(entry => entry.FileName));
    }

    [Fact]
    public async Task ProcessChangesAsync_DeletingImageThatDoesNotBelongToEntity_ReturnsNotBelongsErrorAndChangesNothing()
    {
        var images = BuildImages(0, 1);
        var foreignId = Guid.NewGuid();

        var result = await ProcessAsync(images, new ImageChanges(ToDeleteIds: [images[0].Id, foreignId]));

        Assert.False(result.IsSuccess);
        Assert.Equal(ImageErrors.NotBelongsToEntity(foreignId), result.Error);
        Assert.Equal(2, images.Count);
        _deletedImages.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProcessChangesAsync_DeletingAndAddingTogether_NumbersNewImagesAfterRemainingOnes()
    {
        var images = BuildImages(0, 1, 2);
        SetupDeletedImages();
        SetupUpload(UploadedNames(1));

        var result = await ProcessAsync(images, new ImageChanges(BuildFiles(1), [images[2].Id]));

        Assert.True(result.IsSuccess);
        Assert.Equal([0, 1, 2], images.Select(i => i.Order));
        Assert.Equal("uploaded-0.jpg", images[^1].FileName);
    }

    [Fact]
    public async Task ProcessChangesAsync_UploadFails_PropagatesExceptionWithoutRollback()
    {
        _storage
            .Setup(x => x.UploadRangeAsync(It.IsAny<IReadOnlyList<ImageFile>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(UploadFailureMessage));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => ProcessAsync([], new ImageChanges(BuildFiles(1))));

        _storage.Verify(
            x => x.DeleteByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessChangesAsync_FactoryFailsAfterUpload_DeletesUploadedFilesLogsAndRethrows()
    {
        var uploadedNames = UploadedNames(2);
        using var cts = new CancellationTokenSource();
        SetupUpload(uploadedNames);
        _storage
            .Setup(x => x.DeleteByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => ProcessAsync(
                [],
                new ImageChanges(BuildFiles(2)),
                cts.Token,
                (_, _) => throw new InvalidOperationException(FactoryFailureMessage)));

        Assert.Equal(FactoryFailureMessage, exception.Message);
        _storage.Verify(x => x.DeleteByNamesAsync(uploadedNames, cts.Token), Times.Once);
        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
