using CityPulse.Contracts.Events.Polls.Enums;

namespace CityPulse.Contracts.Events.Polls;

public sealed record PollStatusChangedEvent(
    Guid PollId,
    Guid CityId,
    string Title,
    PollLifecycleStatus Status,
    DateTimeOffset OccurredAt);
