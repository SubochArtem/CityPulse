using CityPulse.Contracts.Querying.Pagination;

namespace Polls.Application.Common.Models;

public abstract class BaseFilter : BasePaginationFilter
{
    public string? SearchTerm { get; set; }
    public bool IncludeImages { get; set; } = true;
}
