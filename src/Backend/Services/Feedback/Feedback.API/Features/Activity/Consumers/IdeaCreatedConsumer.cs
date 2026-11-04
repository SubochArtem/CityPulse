using CityPulse.Contracts.Events.Polls;
using Feedback.API.Domain;
using MassTransit;

namespace Feedback.API.Features.Activity.Consumers;

public sealed class IdeaCreatedConsumer(ActivityStore store) : IConsumer<IdeaCreatedEvent>
{
    public Task Consume(ConsumeContext<IdeaCreatedEvent> context)
    {
        var message = context.Message;

        var entry = new ActivityEntry
        {
            Key = ActivityKeys.IdeaProposed(message.IdeaId),
            Kind = ActivityKind.IdeaProposed,
            CityId = message.CityId,
            PollId = message.PollId,
            PollTitle = message.PollTitle,
            IdeaId = message.IdeaId,
            IdeaTitle = message.Title,
            UserId = message.UserId,
            OccurredAt = message.OccurredAt
        };

        return store.AddAsync(entry, context.CancellationToken);
    }
}
