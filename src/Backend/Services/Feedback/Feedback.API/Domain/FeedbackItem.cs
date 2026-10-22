using MongoDB.Bson.Serialization.Attributes;

namespace Feedback.API.Domain;

public class FeedbackItem
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid IdeaId { get; init; }

    [BsonIgnoreIfNull]
    public string? Text { get; set; }

    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
