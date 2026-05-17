using Microsoft.EntityFrameworkCore;
using Polls.Application.Common.Interfaces;
using Polls.Application.Common.Models;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;
using Polls.Infrastructure.Persistence.Extensions;

namespace Polls.Infrastructure.Persistence.Repositories;

public class PollRepository(ApplicationDbContext context) : Repository<Poll>(context), IPollRepository
{
    public async Task<PagedList<Poll>> GetFilteredAsync(
        PollFilter filter,
        CancellationToken cancellationToken = default)
    {
        return await new PollQueryBuilder(_dbSet.AsNoTracking())
            .WithCityId(filter.CityId)
            .WithType(filter.Type)
            .WithStatus(filter.Status)
            .WithSearchTerm(filter.SearchTerm)
            .WithEndsAtBefore(filter.EndsBefore)
            .IncludeImages(filter.IncludeImages)
            .Build()
            .ToPagedListAsync(filter.Page, filter.PageSize, cancellationToken);
    }

    public async Task<Poll?> GetWithIdeasAsync(
        Guid id,
        IdeaStatus? ideaStatus,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.Images)
            .Include(p => p.Ideas.Where(i => ideaStatus == null || i.Status == ideaStatus))
            .ThenInclude(i => i.Images)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
    
    public async Task<Poll?> GetByIdWithImagesAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
    
    public async Task UpdateStatusByCityAsync(
        Guid cityId, 
        PollStatus source, 
        PollStatus target, 
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken = default)
    {
        await _dbSet
            .Where(p => p.CityId == cityId && p.Status == source)
            .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Status, target)
                    .SetProperty(p => p.UpdatedAt, updatedAt),
                cancellationToken);
    }

    public async Task<IReadOnlyList<Poll>> GetExpiredAsync(
        int batchSize = 100,
        CancellationToken cancellationToken = default)
    {
        return await new PollQueryBuilder(_dbSet)
            .WithStatus(PollStatus.Active)
            .WithEndsAtBefore(DateTimeOffset.UtcNow)
            .Build()
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }
}
