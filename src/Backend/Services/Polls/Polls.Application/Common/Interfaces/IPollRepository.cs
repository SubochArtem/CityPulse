using Polls.Application.Common.Models;
using Polls.Domain.Polls;

namespace Polls.Application.Common.Interfaces;

public interface IPollRepository : IRepository<Poll>
{
    Task<PagedList<Poll>> GetFilteredAsync(
        PollFilter filter,
        CancellationToken cancellationToken = default);

    Task<Poll?> GetWithIdeasAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
