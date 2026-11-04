namespace CityPulse.Contracts.Events.Polls;

public sealed record IdeaDeletedEvent(
    Guid IdeaId,
    Guid PollId,
    Guid CityId,
    DateTimeOffset OccurredAt);
