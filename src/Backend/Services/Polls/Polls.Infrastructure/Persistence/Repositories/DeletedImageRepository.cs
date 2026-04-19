using Microsoft.EntityFrameworkCore;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Images;

namespace Polls.Infrastructure.Persistence.Repositories;

public class DeletedImageRepository(ApplicationDbContext db) : IDeletedImageRepository
{
    public async Task<IReadOnlyList<DeletedImage>> GetBatchAsync(
        int batchSize,
        CancellationToken ct = default)
    {
        return await db.DeletedImages
            .OrderBy(x => x.QueuedAt)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    public void AddRange(IEnumerable<DeletedImage> images)
    {
        db.DeletedImages.AddRange(images);
    }

    public void RemoveRange(IEnumerable<DeletedImage> images)
    {
        db.DeletedImages.RemoveRange(images);
    }
}
