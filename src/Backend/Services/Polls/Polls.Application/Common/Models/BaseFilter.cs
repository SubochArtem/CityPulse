namespace Polls.Application.Common.Models;

public abstract class BaseFilter
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
