namespace Polls.Application.Common.Models;

public abstract class BaseFilter
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 10;

    public int Page { get; set; } = DefaultPage;
    public int PageSize { get; set; } = DefaultPageSize;
    public string? SearchTerm { get; set; }
}
