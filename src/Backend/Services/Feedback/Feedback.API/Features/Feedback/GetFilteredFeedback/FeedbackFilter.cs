using CityPulse.Contracts.Querying.Pagination;

namespace Feedback.API.Features.Feedback.GetFilteredFeedback;

public class FeedbackFilter : BasePaginationFilter
{
    public Guid? IdeaId { get; set; }
    public Guid? UserId { get; set; }
    public FeedbackKind? Kind { get; set; }
    public string? SearchTerm { get; set; }
    public DateTimeOffset? CreatedFrom { get; set; }
    public DateTimeOffset? CreatedTo { get; set; }
}
