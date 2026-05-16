using Polls.Application.Cities.DTOs;

namespace Polls.API.Requests.Cities;

public record CreateCityRequest
{
    public required string Title { get; init; }
    public required CoordinatesDto Coordinates { get; init; }
    public string? Description { get; init; }
}
