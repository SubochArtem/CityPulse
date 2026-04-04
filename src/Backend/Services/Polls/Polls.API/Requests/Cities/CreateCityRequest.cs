using Polls.Application.Cities.DTOs;

namespace Polls.API.Requests.Cities;

public record CreateCityRequest(
    string Name,
    CoordinatesDto Coordinates);
