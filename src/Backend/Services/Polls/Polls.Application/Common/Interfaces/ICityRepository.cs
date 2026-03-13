using Polls.Application.Common.Models;
using Polls.Domain.Cities;

namespace Polls.Application.Common.Interfaces;

public interface ICityRepository : IRepository<City>
{
    Task<PagedList<City>> GetFilteredAsync(
        CityFilter filter,
        CancellationToken cancellationToken = default);
}
