namespace CityPulse.Contracts.Events.Polls;

public sealed record IdeaCreatedEvent(
    Guid IdeaId,
    Guid PollId,
    Guid CityId,
    Guid UserId,
    string Title,
    string PollTitle,
    DateTimeOffset OccurredAt);
