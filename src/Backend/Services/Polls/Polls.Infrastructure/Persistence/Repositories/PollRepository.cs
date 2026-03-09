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
        return await _dbSet
            .Where(p => p.CityId == cityId
                        && (pollType == null || p.Type == pollType)
                        && (pollStatus == null || p.Status == pollStatus))
            .ToListAsync(cancellationToken);
    }
}
