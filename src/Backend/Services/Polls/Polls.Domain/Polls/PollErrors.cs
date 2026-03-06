using Polls.Domain.Common;

namespace Polls.Domain.Polls;

public static class PollErrors
{
    public static Error NotFound => Error.NotFound("Poll not found");
    public static Error CityNotFound => Error.NotFound("City not found");
    public static Error InvalidDates => Error.Validation("End date must be after start date");
    public static Error InvalidBudget => Error.Validation("Budget must be greater than 0");
    public static Error AlreadyFinished => Error.Conflict("Poll is already finished");
}
