using CityPulse.Contracts.Querying.Pagination;
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
    
    Task<Idea?> GetByIdWithImagesAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task UpdateAccessStatusByCityAsync(
        Guid cityId,
        IdeaAccessStatus sourceIdeaAccessStatus,
        IdeaAccessStatus targetIdeaAccessStatus,
        CancellationToken cancellationToken = default);

    Task UpdateAccessStatusByPollIdAsync(
        Guid pollId,
        IdeaAccessStatus sourceIdeaAccessStatus,
        IdeaAccessStatus targetIdeaAccessStatus,
        CancellationToken cancellationToken = default);

    Task UpdateAccessStatusByUserIdAsync(
        Guid userId,
        IdeaAccessStatus sourceIdeaAccessStatus,
        IdeaAccessStatus targetIdeaAccessStatus,
        CancellationToken cancellationToken = default);
}
