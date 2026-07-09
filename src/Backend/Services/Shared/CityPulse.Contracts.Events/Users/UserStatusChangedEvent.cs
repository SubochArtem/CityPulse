using CityPulse.Contracts.Events.Users.Enums;

namespace CityPulse.Contracts.Events.Users;

public sealed record UserStatusChangedEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid UserId,
    UserLifecycleStatus UserLifecycleStatus,
    int Version = 1);
