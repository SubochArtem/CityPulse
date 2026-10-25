using Feedback.API.Domain;
using Feedback.API.Features.Feedback;
using Xunit;

namespace Feedback.Tests.Unit.Mapping.Tests;

public sealed class FeedbackResponseTests
{
    private const string CommentText = "Great idea!";

    private static readonly DateTimeOffset CreatedAt = new(2026, 10, 25, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset UpdatedAt = new(2026, 10, 26, 8, 30, 0, TimeSpan.Zero);

    private static FeedbackItem BuildItem(string? text, DateTimeOffset? updatedAt = null) => new()
    {
        Id = Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        IdeaId = Guid.NewGuid(),
        Text = text,
        CreatedAt = CreatedAt,
        UpdatedAt = updatedAt
    };

    [Fact]
    public void FromEntity_Comment_MapsAllFields()
    {
        var item = BuildItem(CommentText, UpdatedAt);

        var response = FeedbackResponse.FromEntity(item);

        Assert.Equal(item.Id, response.Id);
        Assert.Equal(item.UserId, response.UserId);
        Assert.Equal(item.IdeaId, response.IdeaId);
        Assert.Equal(CommentText, response.Text);
        Assert.Equal(CreatedAt, response.CreatedAt);
        Assert.Equal(UpdatedAt, response.UpdatedAt);
    }

    [Fact]
    public void FromEntity_Like_HasNoText()
    {
        var item = BuildItem(null);

        var response = FeedbackResponse.FromEntity(item);

        Assert.Null(response.Text);
        Assert.Equal(item.IdeaId, response.IdeaId);
    }

    [Fact]
    public void FromEntity_ItemWasNeverEdited_HasNoUpdatedAt()
    {
        var item = BuildItem(CommentText);

        var response = FeedbackResponse.FromEntity(item);

        Assert.Null(response.UpdatedAt);
        Assert.Equal(CreatedAt, response.CreatedAt);
    }
}
