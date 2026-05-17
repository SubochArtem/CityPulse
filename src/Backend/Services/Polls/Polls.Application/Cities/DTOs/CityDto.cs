using Polls.Application.Images.DTOs;

namespace Polls.Application.Cities.DTOs;

public class CityDto
{
    public Guid Id { get; init; }
    public required string Title { get; init; }
    public required CoordinatesDto Coordinates { get; init; }
    public string? Description { get; init; }
    public int Status { get; init; }
    public IReadOnlyList<ImageDto> Images { get; init; } = [];
}
