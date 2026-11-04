using CityPulse.Contracts.Events.Polls;
using MassTransit;

namespace Feedback.API.Features.Activity.Consumers;

public sealed class PollDeletedConsumer(ActivityStore store) : IConsumer<PollDeletedEvent>
{
    public Task Consume(ConsumeContext<PollDeletedEvent> context) =>
        store.DeleteByPollAsync(context.Message.PollId, context.Message.CityId, context.CancellationToken);
}
