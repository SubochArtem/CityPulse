using Feedback.API.Domain;

namespace Feedback.API.Features.Activity;

public record ActivityResponse(
    Guid Id,
    ActivityKind Kind,
    Guid CityId,
    Guid? PollId,
    string? PollTitle,
    Guid? IdeaId,
    string? IdeaTitle,
    Guid? UserId,
    string? Status,
    DateTimeOffset OccurredAt)
{
    public static ActivityResponse FromEntity(ActivityEntry entry) => new(
        entry.Id,
        entry.Kind,
        entry.CityId,
        entry.PollId,
        entry.PollTitle,
        entry.IdeaId,
        entry.IdeaTitle,
        entry.UserId,
        entry.Status,
        entry.OccurredAt);
}
