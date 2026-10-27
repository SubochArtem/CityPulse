using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace CityPulse.Common.Api.Caching;

internal sealed class RedisCacheStore(
    IDistributedCache cache,
    ILogger<RedisCacheStore> logger) : ICacheStore
{
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            var bytes = await cache.GetAsync(key, cancellationToken);

            return bytes is null
                ? null
                : JsonSerializer.Deserialize<T>(bytes);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Failed to read cache key {CacheKey}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
            var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl };

            await cache.SetAsync(key, bytes, options, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Failed to write cache key {CacheKey}", key);
        }
    }
}
