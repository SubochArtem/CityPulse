using Polls.Domain.Common;

namespace Polls.Domain.Polls;

public static class PollErrors
{
    public static Error NotFound(Guid id) =>
         Error.NotFound($"Poll with id '{id}' was not found");

    public static Error AlreadyFinished(Guid id) =>
         Error.Conflict($"Poll with id '{id}' was already finished");

}
