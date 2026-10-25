using Hangfire;
using Polls.Application.Common.Interfaces;
using Polls.Application.Jobs;
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

        unitOfWork.PollScheduleJobs.Create(new PollScheduleJob
        {
            PollId = pollId,
            HangfireJobId = jobId
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public Task CancelAsync(Guid pollId, CancellationToken cancellationToken = default) =>
        CancelRangeAsync([pollId], cancellationToken);

    public async Task CancelRangeAsync(
        IReadOnlyCollection<Guid> pollIds,
        CancellationToken cancellationToken = default)
    {
        if (pollIds.Count == 0)
            return;

        var existing = await unitOfWork.PollScheduleJobs.GetByPollIdsAsync(pollIds, cancellationToken);
        if (existing.Count == 0)
            return;

        foreach (var job in existing)
        {
            backgroundJobClient.Delete(job.HangfireJobId);
            unitOfWork.PollScheduleJobs.Delete(job);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
