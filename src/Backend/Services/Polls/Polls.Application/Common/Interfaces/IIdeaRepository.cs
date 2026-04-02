using Polls.Application.Common.Models;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;

namespace Polls.Application.Common.Interfaces;

public interface IIdeaRepository : IRepository<Idea>
{
    Task<PagedList<Idea>> GetFilteredAsync(
        IdeaFilter filter,
        CancellationToken cancellationToken = default);

    Task<Idea?> GetWithPollAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task UpdateStatusByCityAsync(
        Guid cityId,
        IdeaStatus source,
        IdeaStatus target,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken = default);

    Task UpdateStatusByPollIdAsync(
        Guid pollId,
        IdeaStatus sourceStatus,
        IdeaStatus targetStatus,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken);
}
