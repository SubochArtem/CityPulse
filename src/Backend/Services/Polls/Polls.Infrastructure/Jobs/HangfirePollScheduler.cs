using Hangfire;
using Microsoft.EntityFrameworkCore;
using Polls.Application.Common.Interfaces;
using Polls.Infrastructure.Persistence;
using Polls.Infrastructure.Persistence.Entities;

namespace Polls.Infrastructure.Jobs;

public class HangfirePollScheduler(
    ApplicationDbContext db,
    IBackgroundJobClient backgroundJobClient) : IPollScheduler
{
    public async Task ScheduleAsync(
        Guid pollId, 
        DateTimeOffset endsAt, 
        CancellationToken cancellationToken = default)
    {
        await CancelAsync(pollId, cancellationToken);

        var delay = endsAt - DateTimeOffset.UtcNow;
        
        if (delay < TimeSpan.Zero)
            delay = TimeSpan.Zero;

        var jobId = backgroundJobClient.Schedule<PollStatusJob>(
            job => job.ExecuteAsync(pollId),
            delay);

        db.PollScheduleJobs.Add(new PollScheduleJob
        {
            PollId = pollId,
            HangfireJobId = jobId
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelAsync(Guid pollId, CancellationToken cancellationToken = default)
    {
        var existing = await db.PollScheduleJobs
            .FirstOrDefaultAsync(j => j.PollId == pollId, cancellationToken);

        if (existing is null) 
            return;

        backgroundJobClient.Delete(existing.HangfireJobId);
        db.PollScheduleJobs.Remove(existing);
        await db.SaveChangesAsync(cancellationToken);
    }
}
