using Feedback.API.Domain;

namespace Feedback.Tests.Infrastructure;

internal static class FeedbackItems
{
    private const string DefaultText = "Great idea!";

    public static readonly DateTimeOffset DefaultCreatedAt = new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);

    public static FeedbackItem Comment(
        Guid? userId = null,
        Guid? ideaId = null,
        string text = DefaultText,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? updatedAt = null) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId ?? Guid.NewGuid(),
        IdeaId = ideaId ?? Guid.NewGuid(),
        Text = text,
        CreatedAt = createdAt ?? DefaultCreatedAt,
        UpdatedAt = updatedAt
    };

    public static FeedbackItem Like(
        Guid? userId = null,
        Guid? ideaId = null,
        DateTimeOffset? createdAt = null) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId ?? Guid.NewGuid(),
        IdeaId = ideaId ?? Guid.NewGuid(),
        Text = null,
        CreatedAt = createdAt ?? DefaultCreatedAt
    };
}
