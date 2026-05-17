namespace Polls.Application.Common.Models;

public class PagedList<T>(List<T> items, int page, int pageSize, int totalCount)
{
    public List<T> Items { get; } = items;
    public int Page { get; } = page;
    public int PageSize { get; } = pageSize;
    public int TotalCount { get; } = totalCount;
    public bool HasNextPage => Page * PageSize < TotalCount;
    public bool HasPreviousPage => Page > 1;

    public PagedList<TOut> Map<TOut>(Func<T, TOut> mapper)
    {
        var mappedItems = Items.Select(mapper).ToList();
        return new PagedList<TOut>(mappedItems, Page, PageSize, TotalCount);
    }
}
