using Hangfire;
using Polls.Application.Common.Interfaces;
using Polls.Domain.PollScheduleJob;

namespace Polls.Infrastructure.Jobs;

public class HangfirePollScheduler(
    IUnitOfWork unitOfWork,
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

        unitOfWork.PollScheduleJobs.Add(new PollScheduleJob
        {
            PollId = pollId,
            HangfireJobId = jobId
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelAsync(Guid pollId, CancellationToken cancellationToken = default)
    {
        var existing = await unitOfWork.PollScheduleJobs.GetByPollIdAsync(pollId, cancellationToken);
        if (existing is null)
            return;

        backgroundJobClient.Delete(existing.HangfireJobId);
        unitOfWork.PollScheduleJobs.Remove(existing);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
