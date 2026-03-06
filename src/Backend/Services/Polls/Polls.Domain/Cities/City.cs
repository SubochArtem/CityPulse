using Polls.Domain.Cities.Enums;

namespace Polls.Domain.Cities;

public class City
{
    private City(){}

    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required Coordinates Coordinates { get; set; }
    public string Description { get; set; } = string.Empty;
    public CityStatus Status { get; set; } = CityStatus.Active;
}
