using Polls.Domain.Cities.Enums;
using Polls.Domain.Common;

namespace Polls.Domain.Cities;

public static class CityErrors
{
    public static Error AlreadyExists =>
        Error.Conflict("City already exists");

    public static Error NotFound(Guid id) =>
         Error.NotFound($"City with id '{id}' was not found");
    
    public static Error InvalidStatus(CityStatus status) =>
        Error.Conflict($"City status '{status}' is not supported");
}
