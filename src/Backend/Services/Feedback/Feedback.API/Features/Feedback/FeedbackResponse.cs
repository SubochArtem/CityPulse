using Feedback.API.Domain;

namespace Feedback.API.Features.Feedback;

public record FeedbackResponse(
    Guid Id,
    Guid UserId,
    Guid IdeaId,
    string? Text,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt)
{
    public static FeedbackResponse FromEntity(FeedbackItem item) => new(
        item.Id,
        item.UserId,
        item.IdeaId,
        item.Text,
        item.CreatedAt,
        item.UpdatedAt);
}
