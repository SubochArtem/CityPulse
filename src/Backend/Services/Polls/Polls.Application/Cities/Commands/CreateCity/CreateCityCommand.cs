using Polls.Application.Cities.DTOs;
using Polls.Application.Common.CQRS;

namespace Polls.Application.Cities.Commands.CreateCity;

public record CreateCityCommand(
    string Name,
    CoordinatesDto Coordinates,
    string? Description) : ICommand<CityDto>;
