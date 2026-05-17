namespace Users.Business.DTOs;

public record CityDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public CityStatus Status { get; init; }
}
