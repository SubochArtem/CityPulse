using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;

namespace Polls.Application.Common.Interfaces;

public interface IPollRepository : IRepository<Poll>
{
    Task<IEnumerable<Poll>> GetByCityIdAsync(
        Guid cityId,
        PollType? pollType = null,
        PollStatus? pollStatus = null,
        CancellationToken cancellationToken = default);
}
