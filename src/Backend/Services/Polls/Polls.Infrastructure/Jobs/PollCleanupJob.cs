using Microsoft.Extensions.Logging;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Polls.Enums;

namespace Polls.Infrastructure.Jobs;

public class PollCleanupJob(
    IUnitOfWork unitOfWork)
{
    public async Task ExecuteAsync()
    {
        int processed;

        do
        {
            var expiredPolls = await unitOfWork.Polls.GetExpiredAsync();
            processed = expiredPolls.Count;

            if (processed == 0)
                break;

            foreach (var poll in expiredPolls)
            {
                poll.Status = PollStatus.Inactive;
                unitOfWork.Polls.Update(poll);
            }

            await unitOfWork.SaveChangesAsync();

        } while (processed > 0);
    }
}
