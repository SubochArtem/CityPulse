using CityPulse.Common.Api.Caching;
using ErrorOr;
using Feedback.API.Common.Interfaces;

namespace Feedback.API.Infrastructure.Caching;

public class CachedIdeaService(IIdeaService inner, ICacheStore cache) : IIdeaService
{
    private const string KeyPrefix = "idea-city:";

    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(2);

    public async Task<ErrorOr<Guid>> GetIdeaCityAsync(
        Guid ideaId,
        bool includeOnlyActive,
        CancellationToken cancellationToken = default)
    {
        var key = $"{KeyPrefix}{ideaId}:{includeOnlyActive}";

        var cached = await cache.GetAsync<IdeaCityCache>(key, cancellationToken);
        if (cached is not null)
            return cached.CityId;

        var result = await inner.GetIdeaCityAsync(ideaId, includeOnlyActive, cancellationToken);

        if (!result.IsError)
            await cache.SetAsync(key, new IdeaCityCache(result.Value), CacheTtl, cancellationToken);

        return result;
    }
}
