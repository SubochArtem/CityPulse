namespace Polls.Application.Common.Interfaces;

public interface IPollScheduler
{
    Task ScheduleAsync(
        Guid pollId, 
        DateTimeOffset endsAt, 
        CancellationToken cancellationToken = default);
    
    Task CancelAsync(
        Guid pollId, 
        CancellationToken cancellationToken = default);
}
