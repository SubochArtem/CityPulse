using Microsoft.EntityFrameworkCore;
using Polls.Application.Common.Interfaces;
using Polls.Application.Common.Models;
using Polls.Domain.Cities;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Polls.Enums;
using Polls.Infrastructure.Persistence.Extensions;

namespace Polls.Infrastructure.Persistence.Repositories;

public class CityRepository(ApplicationDbContext context) : Repository<City>(context), ICityRepository
{
    public async Task<PagedList<City>> GetFilteredAsync(
        CityFilter filter,
        CancellationToken cancellationToken = default)
    {
        return await new CityQueryBuilder(_dbSet.AsNoTracking())
            .WithStatus(filter.Status)
            .WithSearchTerm(filter.SearchTerm)
            .Build()
            .ToPagedListAsync(filter.Page, filter.PageSize, cancellationToken);
    }

    public async Task<City?> GetWithPollsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Polls)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task SuspendContentAsync(
        Guid cityId,
        CancellationToken cancellationToken = default)
    {
        await _context.Polls
            .Where(p => p.CityId == cityId && p.Status == PollStatus.Active)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, PollStatus.Suspended), cancellationToken);

        await _context.Ideas
            .Where(i => i.Poll.CityId == cityId && i.Status == IdeaStatus.InPoll)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.Status, IdeaStatus.Suspended), cancellationToken);
    }

    public async Task ActivateContentAsync(
        Guid cityId,
        CancellationToken cancellationToken = default)
    {
        await _context.Polls
            .Where(p => p.CityId == cityId && p.Status == PollStatus.Suspended)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, PollStatus.Active), cancellationToken);

        await _context.Ideas
            .Where(i => i.Poll.CityId == cityId && i.Status == IdeaStatus.Suspended)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.Status, IdeaStatus.InPoll), cancellationToken);
    }
}
