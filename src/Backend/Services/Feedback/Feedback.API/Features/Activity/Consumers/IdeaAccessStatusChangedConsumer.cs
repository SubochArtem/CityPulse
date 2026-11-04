using CityPulse.Contracts.Events.Polls;
using CityPulse.Contracts.Events.Polls.Enums;
using MassTransit;

namespace Feedback.API.Features.Activity.Consumers;

public sealed class IdeaAccessStatusChangedConsumer(ActivityStore store)
    : IConsumer<IdeaAccessStatusChangedEvent>
{
    public Task Consume(ConsumeContext<IdeaAccessStatusChangedEvent> context)
    {
        var message = context.Message;
        if (message.AccessStatus == IdeaAccessLevel.Undefined)
            return Task.CompletedTask;

        return store.SetIdeaHiddenAsync(
            message.IdeaId,
            message.AccessStatus != IdeaAccessLevel.Active,
            context.CancellationToken);
    }
}
