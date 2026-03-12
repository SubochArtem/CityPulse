using Microsoft.EntityFrameworkCore;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Cities;
using Polls.Domain.Cities.Enums;

namespace Polls.Infrastructure.Persistence.Repositories;

public class CityRepository(ApplicationDbContext context)
    : Repository<City>(context), ICityRepository
{
    public async Task<IEnumerable<City>> GetByStatusAsync(
        CityStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.Status == status)
            .ToListAsync(cancellationToken);
    }
}
