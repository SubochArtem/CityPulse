using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polls.Domain.Polls.Enums;
using Polls.Infrastructure.Persistence;

namespace Polls.Infrastructure.Jobs;

public class PollStatusJob(
    ApplicationDbContext db,
    ILogger<PollStatusJob> logger)
{
    public async Task ExecuteAsync(Guid pollId)
    {
        logger.LogInformation("PollStatusJob started for PollId={PollId}", pollId);

        var poll = await db.Polls.FirstOrDefaultAsync(p => p.Id == pollId);
        
        if (poll is null)
        {
            logger.LogWarning("PollStatusJob: Poll {PollId} not found, skipping", pollId);
            return;
        }

        if (poll.Status is not PollStatus.Active)
        {
            logger.LogInformation(
                "PollStatusJob: Poll {PollId} is already in status {Status}, skipping",
                pollId, poll.Status);
            return;
        }

        poll.Status = PollStatus.Inactive;
        
        var scheduleEntry = await db.PollScheduleJobs
            .FirstOrDefaultAsync(j => j.PollId == pollId);
        
        if (scheduleEntry is not null)
            db.PollScheduleJobs.Remove(scheduleEntry);

        await db.SaveChangesAsync();

        logger.LogInformation("Poll {PollId} successfully transitioned to Inactive", pollId);
    }
}
