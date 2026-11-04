namespace Feedback.API.Domain;

public class ActivityEntry
{
    public Guid Id { get; init; }
    public required string Key { get; init; }
    public ActivityKind Kind { get; init; }
    public Guid CityId { get; init; }
    public Guid? PollId { get; init; }
    public string? PollTitle { get; init; }
    public Guid? IdeaId { get; init; }
    public string? IdeaTitle { get; init; }
    public Guid? UserId { get; init; }
    public Guid? FeedbackId { get; init; }
    public string? Status { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
    public bool PollHidden { get; set; }
    public bool IdeaHidden { get; set; }
    public bool UserHidden { get; set; }
}
