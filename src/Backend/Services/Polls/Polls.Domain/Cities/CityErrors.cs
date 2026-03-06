using Polls.Domain.Common;

namespace Polls.Domain.Cities;

public static class CityErrors
{
    public static Error NotFound => Error.NotFound("City not found");
    public static Error AlreadyExists => Error.Conflict("City already exists");
}
