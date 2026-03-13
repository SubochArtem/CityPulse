using Polls.Application.Common.Models;
using Polls.Domain.Ideas;

namespace Polls.Application.Common.Interfaces;

public interface IIdeaRepository : IRepository<Idea>
{
    Task<PagedList<Idea>> GetFilteredAsync(
        IdeaFilter filter,
        CancellationToken cancellationToken = default);
}
