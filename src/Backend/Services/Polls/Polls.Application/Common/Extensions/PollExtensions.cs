using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;

namespace Polls.Application.Common.Extensions;

public static class PollExtensions
{
    public static void EnsureStatusConsistency(this Poll poll)
    {
        if (poll.Status == PollStatus.Active
            && poll.EndsAt <= DateTimeOffset.UtcNow)
            poll.Status = PollStatus.Finished;
    }
}
