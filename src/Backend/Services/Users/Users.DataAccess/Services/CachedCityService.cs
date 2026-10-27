using CityPulse.Common.Api.Caching;
using Users.DataAccess.DTOs;
using Users.DataAccess.Interfaces;

namespace Users.DataAccess.Services;

public class CachedCityService(ICityService inner, ICacheStore cache) : ICityService
{
    private const string KeyPrefix = "city:";

    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(1);

    public async Task<CityDto> GetCityAsync(
        Guid cityId,
        CancellationToken cancellationToken = default)
    {
        var key = $"{KeyPrefix}{cityId}";

        var cached = await cache.GetAsync<CityDto>(key, cancellationToken);
        if (cached is not null)
            return cached;

        var city = await inner.GetCityAsync(cityId, cancellationToken);

        await cache.SetAsync(key, city, CacheTtl, cancellationToken);

        return city;
    }
}
