using Microsoft.EntityFrameworkCore;
using Polls.Application.Common.Interfaces;
using Polls.Domain.PollScheduleJob;

namespace Polls.Infrastructure.Persistence.Repositories;

public class PollScheduleJobRepository(ApplicationDbContext db) : IPollScheduleJobRepository
{
    public async Task<PollScheduleJob?> GetByPollIdAsync(
        Guid pollId,
        CancellationToken cancellationToken = default)
    {
        return await db.PollScheduleJobs
            .FirstOrDefaultAsync(j => j.PollId == pollId, cancellationToken);
    }

    public void Create(PollScheduleJob job)
    {
        db.PollScheduleJobs.Add(job);
    }

    public void Delete(PollScheduleJob job)
    {
        db.PollScheduleJobs.Remove(job);
    }
}
