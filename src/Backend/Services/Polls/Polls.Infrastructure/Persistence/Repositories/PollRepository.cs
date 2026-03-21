using Microsoft.EntityFrameworkCore;
using Polls.Application.Common.Interfaces;
using Polls.Application.Common.Models;
using Polls.Domain.Polls;
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
            .WithCorrelationId(filter.CorrelationId)
            .Build()
            .ToPagedListAsync(filter.Page, filter.PageSize, cancellationToken);
    }

    public async Task<Poll?> GetWithIdeasAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Ideas)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}
