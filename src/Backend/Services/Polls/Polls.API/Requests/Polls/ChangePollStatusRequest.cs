using Polls.Domain.Polls.Enums;

namespace Polls.API.Requests.Polls;

public record ChangePollStatusRequest
{
    public required PollStatus NewStatus { get; init; }
}
