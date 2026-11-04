using CityPulse.Contracts.Events.Users;
using CityPulse.Contracts.Events.Users.Enums;
using MassTransit;

namespace Feedback.API.Features.Activity.Consumers;

public sealed class UserStatusChangedConsumer(ActivityStore store) : IConsumer<UserStatusChangedEvent>
{
    public Task Consume(ConsumeContext<UserStatusChangedEvent> context)
    {
        var message = context.Message;

        return message.UserLifecycleStatus switch
        {
            UserLifecycleStatus.Active => store.SetUserHiddenAsync(message.UserId, false, context.CancellationToken),
            UserLifecycleStatus.Inactive => store.SetUserHiddenAsync(message.UserId, true, context.CancellationToken),
            _ => Task.CompletedTask
        };
    }
}
