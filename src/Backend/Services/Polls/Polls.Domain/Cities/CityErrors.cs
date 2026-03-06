using Polls.Domain.Common;

namespace Polls.Domain.Cities;

public static class CityErrors
{
    public static Error NotFound => Error.NotFound("City not found");
    public static Error AlreadyExists => Error.Conflict("City already exists");
    public static Error InvalidLatitude => Error.Validation("Latitude must be between -90 and 90");
    public static Error InvalidLongitude => Error.Validation("Longitude must be between -180 and 180");
}
