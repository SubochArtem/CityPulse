namespace CityPulse.Contracts.Events.Polls;

public sealed record PollDeletedEvent(
    Guid PollId,
    Guid CityId,
    DateTimeOffset OccurredAt);
