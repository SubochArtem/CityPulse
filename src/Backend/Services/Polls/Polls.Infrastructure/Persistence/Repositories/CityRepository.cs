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
            .IncludeImages(filter.IncludeImages)
            .Build()
            .ToPagedListAsync(filter.Page, filter.PageSize, cancellationToken);
    }

    public async Task<City?> GetWithPollsAsync(
        Guid id,
        PollStatus? status,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.Images)
            .Include(c => c.Polls.Where(p => status == null || p.Status == status))
            .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<City?> GetByIdWithImagesAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Images)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}
