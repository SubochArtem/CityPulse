using ErrorOr;
using Feedback.API.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Feedback.API.Infrastructure.Redis;

public class CachedIdeaService(
    IIdeaService inner,
    IDistributedCache cache,
    ILogger<CachedIdeaService> logger) : IIdeaService
{
    private static readonly DistributedCacheEntryOptions EntryOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
    };

    public async Task<ErrorOr<Guid>> GetIdeaCityAsync(
        Guid ideaId,
        bool includeOnlyActive,
        CancellationToken cancellationToken = default)
    {
        var key = $"idea-city:{ideaId}:{includeOnlyActive}";

        var cached = await TryGetAsync(key, cancellationToken);
        if (cached is not null)
            return Guid.Parse(cached);

        var result = await inner.GetIdeaCityAsync(ideaId, includeOnlyActive, cancellationToken);

        if (!result.IsError)
            await TrySetAsync(key, result.Value.ToString(), cancellationToken);

        return result;
    }

    private async Task<string?> TryGetAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            return await cache.GetStringAsync(key, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Failed to read cache key {CacheKey}", key);
            return null;
        }
    }

    private async Task TrySetAsync(string key, string value, CancellationToken cancellationToken)
    {
        try
        {
            await cache.SetStringAsync(key, value, EntryOptions, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Failed to write cache key {CacheKey}", key);
        }
    }
}
