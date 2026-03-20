using Polls.Application.Cities.DTOs;
using Polls.Application.Common.CQRS;

namespace Polls.Application.Cities.Commands.UpdateCity;

public record UpdateCityCommand(
    Guid Id,
    string Name,
    CoordinatesDto Coordinates,
    string? Description) : ICommand<CityDto>;
