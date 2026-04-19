using Polls.Application.Common.Interfaces;
using Polls.Domain.Polls.Enums;

namespace Polls.Infrastructure.Jobs;

public class PollCleanupJob(
    IUnitOfWork unitOfWork)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        const int batchSize = 100;
        var hasMore = true;

        while (hasMore)
        {
            var expiredPolls = await unitOfWork.Polls.GetExpiredAsync(
                batchSize, 
                cancellationToken);
            hasMore = expiredPolls.Count == batchSize;

            foreach (var poll in expiredPolls)
            {
                poll.Status = PollStatus.Inactive;
                unitOfWork.Polls.Update(poll);
            }

            if (expiredPolls.Count > 0)
                await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
