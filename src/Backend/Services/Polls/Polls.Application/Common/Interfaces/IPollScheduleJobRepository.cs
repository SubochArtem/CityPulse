using Polls.Domain.PollScheduleJob;

namespace Polls.Application.Common.Interfaces;

public interface IPollScheduleJobRepository
{
    Task<PollScheduleJob?> GetByPollIdAsync(
        Guid pollId,
        CancellationToken cancellationToken = default);

    void Create(PollScheduleJob job);

    void Delete(PollScheduleJob job);
}
