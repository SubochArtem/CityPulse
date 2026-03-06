using Polls.Domain.Cities.Enums;
using Polls.Domain.Common;

namespace Polls.Domain.Cities;

public class City : EntityBase
{
    public required string Name { get; set; }
    public required Coordinates Coordinates { get; set; }
    public string Description { get; set; } = string.Empty;
    public CityStatus Status { get; set; } = CityStatus.Active;
}
