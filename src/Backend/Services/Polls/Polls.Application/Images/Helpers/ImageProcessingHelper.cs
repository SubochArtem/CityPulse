using MediatR;
using Microsoft.Extensions.Logging;
using Polls.Application.Common.Interfaces;
using Polls.Application.Common.Models;
using Polls.Application.Images.Validators;
using Polls.Domain.Common;
using Polls.Domain.Images;

namespace Polls.Application.Images.Helpers;

public static class ImageProcessingHelper
{
    public static async Task<Result<Unit>> ProcessChangesAsync<TImage>(
        ICollection<TImage> currentImages,
        IUnitOfWork unitOfWork,
        IImageStorageService storageService,
        ILogger logger,
        ImageFactory<TImage> createImageFactory,
        ImageChanges imageChanges,
        CancellationToken cancellationToken = default)
        where TImage : Image
    {
        if (imageChanges.ToAdd is { Count: > 0 })
        {
            var currentCount = currentImages.Count - (imageChanges.ToDeleteIds?.Count ?? 0);
            var validationResult = ImageValidator.Validate(imageChanges.ToAdd, currentCount);
            if (!validationResult.IsSuccess) return validationResult.Error;
        }
        
        var deletionResult = HandleDeletions(currentImages, unitOfWork, imageChanges.ToDeleteIds);
        if (!deletionResult.IsSuccess) return deletionResult.Error;

        await HandleAdditionsAsync(
            currentImages,
            storageService,
            logger,
            createImageFactory,
            imageChanges.ToAdd,
            cancellationToken);

        return Unit.Value;
    }

    private static Result<Unit> HandleDeletions<TImage>(
        ICollection<TImage> currentImages,
        IUnitOfWork unitOfWork,
        IReadOnlyList<Guid>? toDeleteIds) where TImage : Image
    {
        if (toDeleteIds is not { Count: > 0 }) return Unit.Value;

        var toDelete = currentImages.Where(i => toDeleteIds.Contains(i.Id)).ToList();

        if (toDelete.Count != toDeleteIds.Count)
        {
            var missingId = toDeleteIds.First(id => toDelete.All(i => i.Id != id));
            return ImageErrors.NotBelongsToEntity(missingId);
        }

        var deletedEntries = toDelete.Select(i => new DeletedImage { FileName = i.FileName });
        unitOfWork.DeletedImages.AddRange(deletedEntries);

        foreach (var img in toDelete)
            currentImages.Remove(img);

        return Unit.Value;
    }

    private static async Task HandleAdditionsAsync<TImage>(
        ICollection<TImage> currentImages,
        IImageStorageService storageService,
        ILogger logger,
        ImageFactory<TImage> factory,
        IReadOnlyList<ImageFile>? imagesToAdd,
        CancellationToken ct) where TImage : Image
    {
        if (imagesToAdd is not { Count: > 0 }) return;

        IReadOnlyList<string> uploadedFileNames = [];
        try
        {
            uploadedFileNames = await storageService.UploadRangeAsync(imagesToAdd, ct);

            var maxOrder = currentImages.Any() ? currentImages.Max(i => i.Order) : -1;

            for (var i = 0; i < uploadedFileNames.Count; i++)
            {
                var newImage = factory(uploadedFileNames[i], maxOrder + i + 1);
                currentImages.Add(newImage);
            }
        }
        catch (Exception ex)
        {
            if (uploadedFileNames.Count > 0)
            {
                logger.LogError(ex, "Rolling back {Count} uploaded images due to error", uploadedFileNames.Count);
                await storageService.DeleteByNamesAsync(uploadedFileNames, ct);
            }
            throw;
        }
    }
}
