using Polls.Application.Polls.DTOs;

namespace Polls.Application.Cities.DTOs;

public record CityWithPollsDto(
    Guid Id,
    string Name,
    double Latitude,
    double Longitude,
    string? Description,
    int Status,
    IReadOnlyCollection<PollDto> Polls);
