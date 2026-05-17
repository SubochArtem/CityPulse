using Polls.Application.Cities.DTOs;

namespace Polls.API.Requests.Cities;

public record UpdateCityRequest
{
    public required string Title { get; init; }
    public required CoordinatesDto Coordinates { get; init; }
    public string? Description { get; init; }
    public IFormFileCollection? ImagesToAdd { get; init; } 
    public List<Guid>? ImagesToDelete { get; init; }
}
