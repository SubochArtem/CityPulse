using Polls.Domain.PollScheduleJob;

namespace Polls.Application.Common.Interfaces;

public interface IPollScheduleJobRepository
{
    Task<PollScheduleJob?> GetByPollIdAsync(
        Guid pollId,
        CancellationToken cancellationToken = default);

    void Add(PollScheduleJob job);

    void Remove(PollScheduleJob job);
}
