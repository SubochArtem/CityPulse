using CityPulse.Contracts.Events.Polls.Enums;

namespace CityPulse.Contracts.Events.Polls;

public sealed record IdeaApprovalStatusChangedEvent(
    Guid IdeaId,
    Guid PollId,
    Guid CityId,
    Guid UserId,
    string Title,
    string PollTitle,
    IdeaReviewStatus ApprovalStatus,
    DateTimeOffset OccurredAt);
