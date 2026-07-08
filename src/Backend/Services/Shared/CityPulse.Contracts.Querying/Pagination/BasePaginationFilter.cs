namespace CityPulse.Contracts.Querying.Pagination;

public abstract class BasePaginationFilter
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 10;

    public int Page { get; set; } = DefaultPage;
    public int PageSize { get; set; } = DefaultPageSize;
}
