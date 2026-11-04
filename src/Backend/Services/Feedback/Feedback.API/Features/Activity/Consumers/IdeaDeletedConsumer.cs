using CityPulse.Contracts.Events.Polls;
using MassTransit;

namespace Feedback.API.Features.Activity.Consumers;

public sealed class IdeaDeletedConsumer(ActivityStore store) : IConsumer<IdeaDeletedEvent>
{
    public Task Consume(ConsumeContext<IdeaDeletedEvent> context) =>
        store.DeleteByIdeaAsync(context.Message.IdeaId, context.Message.CityId, context.CancellationToken);
}
