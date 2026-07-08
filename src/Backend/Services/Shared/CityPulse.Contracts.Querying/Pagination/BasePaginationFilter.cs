namespace CityPulse.Contracts.Querying.Pagination;

public abstract class BasePaginationFilter
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
