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
        IdeaAccessStatus sourceIdeaAccessStatus,
        IdeaAccessStatus targetIdeaAccessStatus,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken = default)
    {
        await _dbSet
            .Where(i => i.Poll.CityId == cityId && i.AccessStatus == sourceIdeaAccessStatus)
            .ExecuteUpdateAsync(s => s
                    .SetProperty(i => i.AccessStatus, targetIdeaAccessStatus)
                    .SetProperty(i => i.UpdatedAt, updatedAt),
                cancellationToken);
    }
    
    public async Task UpdateAccessStatusByPollIdAsync(
        Guid pollId,
        IdeaAccessStatus sourceIdeaAccessStatus,
        IdeaAccessStatus targetIdeaAccessStatus,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken = default)
    {
        await _dbSet
            .Where(i => i.PollId == pollId && i.AccessStatus == sourceIdeaAccessStatus)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(i => i.AccessStatus, targetIdeaAccessStatus)
                    .SetProperty(i => i.UpdatedAt, updatedAt), 
                cancellationToken);
    }
    
    public async Task UpdateAccessStatusByAuthorIdAsync(
        Guid userId,
        IdeaAccessStatus sourceIdeaAccessStatus,
        IdeaAccessStatus targetIdeaAccessStatus,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken = default)
    {
        await _dbSet
            .Where(i => i.UserId == userId && i.AccessStatus == sourceIdeaAccessStatus)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(i => i.AccessStatus, targetIdeaAccessStatus)
                    .SetProperty(i => i.UpdatedAt, updatedAt),
                cancellationToken);
    }
}
