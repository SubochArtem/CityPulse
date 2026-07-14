using MassTransit;
using Users.DataAccess.Interfaces;

namespace Users.DataAccess.Messaging;

public sealed class EventPublisherAdapter(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) 
        where T : class
    {
        await publishEndpoint.Publish(message, cancellationToken);
    }
}
