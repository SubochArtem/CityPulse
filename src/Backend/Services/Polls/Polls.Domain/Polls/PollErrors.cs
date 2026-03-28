using Polls.Domain.Common;

namespace Polls.Domain.Polls;

public static class PollErrors
{
    public static Error NotFound(Guid id) =>
         Error.NotFound($"Poll with id '{id}' was not found");

    public static Error AlreadyFinished(Guid id) =>
         Error.Conflict($"Poll with id '{id}' was already finished");
    
    public static Error AlreadyExists(Guid cityId) =>
        Error.Conflict($"Active poll in city '{cityId}' was already created");
    
    public static Error MaxDurationExceeded(int maxDays) =>
        Error.Conflict($"Poll duration cannot exceed {maxDays} days from its creation date");
    
    public static Error UpdatePeriodExpired(int maxDays) =>
        Error.Conflict($"The editing period for this poll ({maxDays} day(s)) has expired");
    
    public static Error NotFromUserCity(Guid? pollId = null) =>
        Error.Forbidden($"Cannot interact with poll '{pollId}' because it belongs to a different city");
}
