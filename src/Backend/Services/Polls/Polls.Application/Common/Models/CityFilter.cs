using Polls.Domain.Cities.Enums;

namespace Polls.Application.Common.Models;

public class CityFilter : BaseFilter
{
    public CityStatus? Status { get; set; }
    public bool IncludeImages { get; set; } = true;
}
