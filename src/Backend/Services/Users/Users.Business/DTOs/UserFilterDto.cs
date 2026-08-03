using CityPulse.Contracts.Querying.Pagination;

namespace Users.Business.DTOs;

public class UserFilterDto: BasePaginationFilter
{
    public string? Nickname { get; set; }
    public Guid? CityId { get; set; }
}
