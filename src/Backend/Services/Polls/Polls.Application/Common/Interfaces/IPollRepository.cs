using Polls.Application.Common.Models;
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
        CancellationToken cancellationToken = default);

    Task UpdateStatusByCityAsync(
        Guid cityId,
        PollStatus source,
        PollStatus target,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken = default);
}
