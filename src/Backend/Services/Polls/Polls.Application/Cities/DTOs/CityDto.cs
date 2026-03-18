namespace Polls.Application.Cities.DTOs;

public record CityDto(
    Guid Id,
    string Name,
    CoordinatesDto Coordinates,
    string? Description,
    int Status);
