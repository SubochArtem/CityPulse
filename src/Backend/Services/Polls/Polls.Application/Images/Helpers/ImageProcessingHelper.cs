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
        IReadOnlyList<ImageFile>? imagesToAdd = null,
        IReadOnlyList<Guid>? imagesToDeleteIds = null,
        CancellationToken cancellationToken = default)
        where TImage : Image
    {
        if (imagesToAdd is { Count: > 0 })
        {
            var currentCount = currentImages.Count - (imagesToDeleteIds?.Count ?? 0);
            var validationResult = ImageValidator.Validate(imagesToAdd, currentCount);
            if (!validationResult.IsSuccess)
                return validationResult.Error;
        }

        if (imagesToDeleteIds is { Count: > 0 })
        {
            var toDelete = currentImages
                .Where(i => imagesToDeleteIds.Contains(i.Id))
                .ToList();

            if (toDelete.Count != imagesToDeleteIds.Count)
            {
                var missingId = imagesToDeleteIds
                    .First(id => toDelete.All(i => i.Id != id));
                return ImageErrors.NotBelongsToEntity(missingId);
            }

            var deletedImages = toDelete
                .Select(i => new DeletedImage { FileName = i.FileName })
                .ToList();

            unitOfWork.DeletedImages.AddRange(deletedImages);

            foreach (var img in toDelete)
                currentImages.Remove(img);
        }

        IReadOnlyList<string> uploadedFileNames = [];
        if (imagesToAdd is { Count: > 0 })
        {
            try
            {
                uploadedFileNames = await storageService.UploadRangeAsync(imagesToAdd, cancellationToken);

                var currentMaxOrder = currentImages.Any()
                    ? currentImages.Max(i => i.Order)
                    : -1;

                for (var i = 0; i < uploadedFileNames.Count; i++)
                {
                    var newImage = createImageFactory(uploadedFileNames[i], currentMaxOrder + i + 1);
                    currentImages.Add(newImage);
                }
            }
            catch (Exception ex)
            {
                if (uploadedFileNames.Count > 0)
                {
                    logger.LogError(
                        ex,
                        "Rolling back {Count} uploaded images",
                        uploadedFileNames.Count);

                    await storageService.DeleteByNamesAsync(uploadedFileNames, cancellationToken);
                }
                throw;
            }
        }
        return Unit.Value;
    }
}
