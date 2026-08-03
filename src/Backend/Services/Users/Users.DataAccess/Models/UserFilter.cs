using CityPulse.Contracts.Querying.Pagination;

namespace Users.DataAccess.Models;

public class UserFilter: BasePaginationFilter
{
    public string? Nickname { get; set; }
    public Guid? CityId { get; set; }
}
