using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;

namespace Polls.Application.Common.Interfaces;

public interface IIdeaRepository : IRepository<Idea>
{
    Task<IEnumerable<Idea>> GetByPollIdAsync(
        Guid pollId,
        IdeaStatus? status = null,
        CancellationToken cancellationToken = default);
}
