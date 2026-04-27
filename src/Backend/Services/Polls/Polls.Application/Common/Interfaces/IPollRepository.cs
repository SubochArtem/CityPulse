using Polls.Application.Common.Models;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;

namespace Polls.Application.Common.Interfaces;

public interface IPollRepository : IRepository<Poll>
{
    Task<PagedList<Poll>> GetFilteredAsync(
        PollFilter filter,
        CancellationToken cancellationToken = default);

    Task<Poll?> GetWithIdeasAsync(
        Guid id,
        IdeaStatus? ideaStatus,
        CancellationToken cancellationToken = default);
    
    Task<Poll?> GetByIdWithImagesAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task UpdateStatusByCityAsync(
        Guid cityId,
        PollStatus source,
        PollStatus target,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<Poll>> GetExpiredAsync(
        int batchSize = 100, 
        CancellationToken cancellationToken =  default);
}
