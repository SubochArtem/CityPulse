using Polls.Application.Common.Interfaces;

namespace Polls.Infrastructure.Jobs;

public class ImageCleanupJob(
    IUnitOfWork unitOfWork,
    IImageStorageService storageService)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        const int batchSize = 100;
        var hasMore = true;

        while (hasMore)
        {
            var batch = await unitOfWork.DeletedImages.GetBatchAsync(batchSize, cancellationToken);
            hasMore = batch.Count == batchSize;

            if (batch.Count == 0)
                break;

            var fileNames = batch.Select(x => x.FileName).ToList();

            await storageService.DeleteByNamesAsync(fileNames, cancellationToken);
            unitOfWork.DeletedImages.RemoveRange(batch);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
