using CityPulse.Contracts.Querying.Pagination;
using Microsoft.EntityFrameworkCore;
using Polls.Application.Common.Interfaces;
using Polls.Application.Common.Models;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;
using Polls.Infrastructure.Persistence.Extensions;

namespace Polls.Infrastructure.Persistence.Repositories;

public class IdeaRepository(ApplicationDbContext context)
    : Repository<Idea>(context), IIdeaRepository
{
    public async Task<PagedList<Idea>> GetFilteredAsync(
        IdeaFilter filter,
        CancellationToken cancellationToken = default)
    {
        return await new IdeaQueryBuilder(_dbSet.AsNoTracking())
            .WithPollId(filter.PollId)
            .WithAccessStatus(filter.AccessStatus)
            .WithApprovalStatus(filter.ApprovalStatus)
            .WithSearchTerm(filter.SearchTerm)
            .IncludeImages(filter.IncludeImages)
            .Build()
            .ToPagedListAsync(filter.Page, filter.PageSize, cancellationToken);
    }

    public async Task<Idea?> GetWithPollAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AsSplitQuery()
            .Include(i => i.Images)
            .Include(i => i.Poll)
            .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }
    
    public async Task<Idea?> GetByIdWithImagesAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.Images)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }
    
    public async Task UpdateAccessStatusByCityAsync(
        Guid cityId,
        AccessStatus sourceAccessStatus,
        AccessStatus targetAccessStatus,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken = default)
    {
        await _dbSet
            .Where(i => i.Poll.CityId == cityId && i.AccessStatus == sourceAccessStatus)
            .ExecuteUpdateAsync(s => s
                    .SetProperty(i => i.AccessStatus, targetAccessStatus)
                    .SetProperty(i => i.UpdatedAt, updatedAt),
                cancellationToken);
    }
    
    public async Task UpdateAccessStatusByPollIdAsync(
        Guid pollId,
        AccessStatus sourceAccessStatus,
        AccessStatus targetAccessStatus,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken = default)
    {
        await _dbSet
            .Where(i => i.PollId == pollId && i.AccessStatus == sourceAccessStatus)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(i => i.AccessStatus, targetAccessStatus)
                    .SetProperty(i => i.UpdatedAt, updatedAt), 
                cancellationToken);
    }
}
