namespace Polls.Application.Common.Interfaces;

public interface IIntegrationEventPublisher
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
        where T : class;
}
