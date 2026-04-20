using Polls.Application.Cities.DTOs;
using Polls.Application.Common.CQRS;
using Polls.Application.Common.Models;

namespace Polls.Application.Cities.Commands.UpdateCity;

public record UpdateCityCommand(
    Guid Id,
    string Title,
    CoordinatesDto Coordinates,
    string? Description,
    IReadOnlyList<ImageFile>? ImagesToAdd = null,
    IReadOnlyList<Guid>? ImagesToDelete = null) : ICommand<CityDto>;
