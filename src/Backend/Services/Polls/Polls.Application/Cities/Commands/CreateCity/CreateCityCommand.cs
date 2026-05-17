using Polls.Application.Cities.DTOs;
using Polls.Application.Common.CQRS;
using Polls.Application.Common.Models;

namespace Polls.Application.Cities.Commands.CreateCity;

public record CreateCityCommand(
    string Title,
    CoordinatesDto Coordinates,
    string? Description,
    IReadOnlyList<ImageFile>? Images = null) : ICommand<CityDto>;
