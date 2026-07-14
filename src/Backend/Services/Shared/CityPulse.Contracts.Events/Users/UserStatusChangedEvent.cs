using CityPulse.Contracts.Events.Users.Enums;

namespace CityPulse.Contracts.Events.Users;

public sealed record UserStatusChangedEvent(
    Guid UserId,
    UserLifecycleStatus UserLifecycleStatus);
