using Microsoft.EntityFrameworkCore;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;

namespace Polls.Infrastructure.Persistence.Repositories;

public class PollRepository(ApplicationDbContext context) : Repository<Poll>(context), IPollRepository
{
    public async Task<IEnumerable<Poll>> GetByCityIdAsync(
        Guid cityId,
        PollType? pollType = null,
        PollStatus? pollStatus = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(p => p.CityId == cityId);

        if (pollType is not null)
            query = query.Where(p => p.Type == pollType);

        if (pollStatus is not null)
            query = query.Where(p => p.Status == pollStatus);

        return await query.ToListAsync(cancellationToken);
    }
}
