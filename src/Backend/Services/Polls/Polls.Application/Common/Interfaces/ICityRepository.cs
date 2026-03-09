using Polls.Domain.Cities;
using Polls.Domain.Cities.Enums;

namespace Polls.Application.Common.Interfaces;

public interface ICityRepository : IRepository<City>
{
    Task<IEnumerable<City>> GetByStatusAsync(
        CityStatus? status = null,
        CancellationToken cancellationToken = default);
}
