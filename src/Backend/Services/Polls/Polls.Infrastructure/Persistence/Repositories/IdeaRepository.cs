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
        return await _dbSet
            .Where(i => i.PollId == pollId
                        && (status == null || i.Status == status))
            .ToListAsync(cancellationToken);
    }
}
