using Polls.Application.Cities.DTOs;

namespace Polls.API.Requests.Cities;

public record UpdateCityRequest(
    string Name,
    CoordinatesDto Coordinates);
