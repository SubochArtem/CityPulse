using Polls.Application.Common.Models;
using Polls.Domain.Cities;

namespace Polls.Application.Common.Interfaces;

public interface ICityRepository : IRepository<City>
{
    Task<PagedList<City>> GetFilteredAsync(
        CityFilter filter,
        CancellationToken cancellationToken = default);

    Task<City?> GetWithPollsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task SuspendContentAsync(
        Guid cityId,
        CancellationToken cancellationToken = default);

    Task ActivateContentAsync(
        Guid cityId,
        CancellationToken cancellationToken = default);
}
