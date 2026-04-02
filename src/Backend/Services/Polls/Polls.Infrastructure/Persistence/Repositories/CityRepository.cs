using Microsoft.EntityFrameworkCore;
using Polls.Application.Common.Interfaces;
using Polls.Application.Common.Models;
using Polls.Domain.Cities;
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
        PollStatus? status,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => new PollQueryBuilder(c.Polls.AsQueryable()) 
                .WithStatus(status)                               
                .Build())                       
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}
