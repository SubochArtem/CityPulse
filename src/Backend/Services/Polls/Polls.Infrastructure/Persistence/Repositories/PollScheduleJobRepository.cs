using Microsoft.EntityFrameworkCore;
using Polls.Application.Common.Interfaces;
using Polls.Domain.PollScheduleJob;

namespace Polls.Infrastructure.Persistence.Repositories;

public class PollScheduleJobRepository(ApplicationDbContext db) : IPollScheduleJobRepository
{
    public async Task<PollScheduleJob?> GetByPollIdAsync(
        Guid pollId,
        CancellationToken ct = default)
    {
        return await db.PollScheduleJobs
            .FirstOrDefaultAsync(j => j.PollId == pollId, ct);
    }

    public void Add(PollScheduleJob job)
    {
        db.PollScheduleJobs.Add(job);
    }

    public void Remove(PollScheduleJob job)
    {
        db.PollScheduleJobs.Remove(job);
    }
}
