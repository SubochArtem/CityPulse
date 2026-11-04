using MassTransit;
using Polls.Application.Common.Interfaces;

namespace Polls.Infrastructure.Messaging;

public sealed class MassTransitEventPublisher(IPublishEndpoint publishEndpoint) : IIntegrationEventPublisher
{
    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
        where T : class =>
        publishEndpoint.Publish(message, cancellationToken);
}
