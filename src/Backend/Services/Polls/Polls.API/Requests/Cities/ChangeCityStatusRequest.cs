using Polls.Domain.Cities.Enums;

namespace Polls.API.Requests.Cities;

public record ChangeCityStatusRequest
{
    public required CityStatus NewStatus { get; init; }
}
