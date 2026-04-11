using Polls.Application.Common.Interfaces;
using Polls.Domain.Polls.Enums;

namespace Polls.Infrastructure.Jobs;

public class PollStatusJob(
    IUnitOfWork unitOfWork)
{
    public async Task ExecuteAsync(Guid pollId)
    {
        var poll = await unitOfWork.Polls.GetByIdAsync(pollId);
        if (poll is null)
            return;

        if (poll.Status is not PollStatus.Active)
            return;

        poll.Status = PollStatus.Inactive;
        unitOfWork.Polls.Update(poll);

        var scheduleEntry = await unitOfWork.PollScheduleJobs.GetByPollIdAsync(pollId);
        if (scheduleEntry is not null)
            unitOfWork.PollScheduleJobs.Remove(scheduleEntry);

        await unitOfWork.SaveChangesAsync();
    }
}
