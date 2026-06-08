using Users.Business.DTOs;

namespace Users.Business.Interfaces;

public interface ICityService
{
    Task<CityDto?> GetCityAsync(
        Guid cityId,
        CancellationToken cancellationToken = default);
}
