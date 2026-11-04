using CityPulse.Contracts.Events.Polls;
using MassTransit;

namespace Polls.Tests.Infrastructure;

public sealed class ActivityEventsProbeConsumer :
    IConsumer<IdeaCreatedEvent>,
    IConsumer<IdeaApprovalStatusChangedEvent>
{
    public Task Consume(ConsumeContext<IdeaCreatedEvent> context) => Task.CompletedTask;

    public Task Consume(ConsumeContext<IdeaApprovalStatusChangedEvent> context) => Task.CompletedTask;
}
