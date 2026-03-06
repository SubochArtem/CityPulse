using Polls.Domain.Common;

namespace Polls.Domain.Polls;

public static class PollErrors
{
    public static Error NotFound => Error.NotFound("Poll not found");
    public static Error AlreadyFinished => Error.Conflict("Poll is already finished");
}
