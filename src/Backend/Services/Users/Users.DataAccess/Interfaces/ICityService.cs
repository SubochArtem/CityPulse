using Users.DataAccess.DTOs;

namespace Users.DataAccess.Interfaces;

public interface ICityService
{
    Task<CityDto> GetCityAsync(
        Guid cityId,
        CancellationToken cancellationToken = default);
}
