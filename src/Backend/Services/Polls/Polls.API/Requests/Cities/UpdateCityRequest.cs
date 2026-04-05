using Polls.Application.Cities.DTOs;

namespace Polls.API.Requests.Cities;

public record UpdateCityRequest(
    string Title,
    CoordinatesDto Coordinates,
    string? Description);
