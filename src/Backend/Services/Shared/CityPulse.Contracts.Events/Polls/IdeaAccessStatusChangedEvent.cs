using CityPulse.Contracts.Events.Polls.Enums;

namespace CityPulse.Contracts.Events.Polls;

public sealed record IdeaAccessStatusChangedEvent(
    Guid IdeaId,
    Guid PollId,
    Guid CityId,
    IdeaAccessLevel AccessStatus,
    DateTimeOffset OccurredAt);
