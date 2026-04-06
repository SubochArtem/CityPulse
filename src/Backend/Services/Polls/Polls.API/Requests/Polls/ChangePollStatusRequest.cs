using Polls.Domain.Polls.Enums;

namespace Polls.API.Requests.Polls;

public record ChangePollStatusRequest(
    PollStatus NewStatus);
