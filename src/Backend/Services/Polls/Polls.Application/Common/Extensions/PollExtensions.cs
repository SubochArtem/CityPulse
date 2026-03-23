using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;

namespace Polls.Application.Common.Extensions;

public static class PollExtensions
{
    public static bool IsOpen(this Poll poll)
    {
        return poll.Status == PollStatus.Active 
               && poll.EndsAt > DateTimeOffset.UtcNow;
    }
}
