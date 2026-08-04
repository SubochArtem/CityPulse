namespace Users.DataAccess.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<T>(
        T message, 
        CancellationToken cancellationToken = default) 
        where T : class;
}
