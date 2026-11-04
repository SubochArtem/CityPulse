namespace Feedback.API.Domain;

public enum ActivityKind
{
    PollOpened = 1,
    PollClosed = 2,
    IdeaProposed = 3,
    IdeaReviewed = 4,
    IdeaCommented = 5
}
