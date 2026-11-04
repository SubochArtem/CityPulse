namespace Feedback.API.Features.Activity.GetActivity;

public class ActivityFilter
{
    public const int DefaultPageSize = 20;

    public Guid? CityId { get; set; }
    public string? Cursor { get; set; }
    public int PageSize { get; set; } = DefaultPageSize;
}
