using System.Linq.Expressions;
using CityPulse.Contracts.Querying.Pagination;
using Microsoft.EntityFrameworkCore;
using Polls.Application.Common.Interfaces;
using Polls.Application.Common.Models;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;
using Polls.Infrastructure.Persistence.Extensions;

namespace Polls.Infrastructure.Persistence.Repositories;

public class IdeaRepository(ApplicationDbContext context, IDateTimeProvider dateTimeProvider)
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

    public Task UpdateAccessStatusByCityAsync(
        Guid cityId,
        IdeaAccessStatus sourceIdeaAccessStatus,
        IdeaAccessStatus targetIdeaAccessStatus,
        CancellationToken cancellationToken = default)
    {
        return UpdateAccessStatusAsync(
            i => i.Poll.CityId == cityId,
            sourceIdeaAccessStatus,
            targetIdeaAccessStatus,
            cancellationToken);
    }

    public Task UpdateAccessStatusByPollIdAsync(
        Guid pollId,
        IdeaAccessStatus sourceIdeaAccessStatus,
        IdeaAccessStatus targetIdeaAccessStatus,
        CancellationToken cancellationToken = default)
    {
        return UpdateAccessStatusAsync(
            i => i.PollId == pollId,
            sourceIdeaAccessStatus,
            targetIdeaAccessStatus,
            cancellationToken);
    }

    public Task UpdateAccessStatusByUserIdAsync(
        Guid userId,
        IdeaAccessStatus sourceIdeaAccessStatus,
        IdeaAccessStatus targetIdeaAccessStatus,
        CancellationToken cancellationToken = default)
    {
        return UpdateAccessStatusAsync(
            i => i.UserId == userId,
            sourceIdeaAccessStatus,
            targetIdeaAccessStatus,
            cancellationToken);
    }

    private async Task UpdateAccessStatusAsync(
        Expression<Func<Idea, bool>> scopeFilter,
        IdeaAccessStatus sourceIdeaAccessStatus,
        IdeaAccessStatus targetIdeaAccessStatus,
        CancellationToken cancellationToken)
    {
        await _dbSet
            .Where(scopeFilter)
            .Where(i => i.AccessStatus == sourceIdeaAccessStatus)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(i => i.AccessStatus, targetIdeaAccessStatus)
                    .SetProperty(i => i.UpdatedAt, dateTimeProvider.UtcNow),
                cancellationToken);
    }
}
