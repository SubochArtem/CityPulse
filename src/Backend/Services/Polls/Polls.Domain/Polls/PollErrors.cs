using Polls.Domain.Common;

namespace Polls.Domain.Polls;

public static class PollErrors
{
    public static Error NotFound(Guid id)
    {
        return Error.NotFound($"Poll with id '{id}' was not found");
    }

    public static Error AlreadyFinished(Guid id)
    {
        return Error.Conflict($"Poll with id '{id}' was already finished");
    }

    public static Error AlreadyExists(Guid cityId)
    {
        return Error.Conflict($"Active poll in city '{cityId}' was already created");
    }

    public static Error MaxDurationExceeded(int maxDays)
    {
        return Error.Conflict($"Poll duration cannot exceed {maxDays} days from its creation date");
    }
}
