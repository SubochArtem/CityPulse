using Microsoft.EntityFrameworkCore;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;

namespace Polls.Infrastructure.Persistence.Repositories;

public class IdeaRepository(ApplicationDbContext context)
    : Repository<Idea>(context), IIdeaRepository
{
    public async Task<IEnumerable<Idea>> GetByPollIdAsync(
        Guid pollId,
        IdeaStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(i => i.PollId == pollId);

        if (status is not null)
            query = query.Where(i => i.Status == status);

        return await query.ToListAsync(cancellationToken);
    }
}
