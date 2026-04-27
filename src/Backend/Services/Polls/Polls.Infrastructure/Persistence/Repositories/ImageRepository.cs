using Microsoft.EntityFrameworkCore;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Images;

namespace Polls.Infrastructure.Persistence.Repositories;

public class ImageRepository<TImage>(ApplicationDbContext db) : IImageRepository<TImage> 
    where TImage : Image
{
    private readonly DbSet<TImage> _dbSet = db.Set<TImage>();

    public async Task<IReadOnlyList<TImage>> GetByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken ct = default)
    {
        return await _dbSet
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(ct);
    }

    public void AddRange(IEnumerable<TImage> images)
    {
        _dbSet.AddRange(images);
    }

    public void RemoveRange(IEnumerable<TImage> images)
    {
        _dbSet.RemoveRange(images);
    }
}
