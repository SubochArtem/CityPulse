namespace Users.DataAccess.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<TMessage>(
        TMessage message, 
        CancellationToken cancellationToken = default) 
        where TMessage : class;
}
