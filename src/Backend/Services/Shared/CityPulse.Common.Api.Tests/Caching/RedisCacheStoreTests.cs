using System.Text;
using System.Text.Json;
using CityPulse.Common.Api.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace CityPulse.Common.Api.Tests.Caching;

public sealed class RedisCacheStoreTests
{
    private const string Key = "city:1";
    private const string SampleName = "Minsk";
    private const string InvalidJson = "not a json";
    private const string RedisDownMessage = "redis down";

    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(1);

    private readonly Mock<IDistributedCache> _cache = new(MockBehavior.Strict);
    private readonly RedisCacheStore _sut;

    public RedisCacheStoreTests()
    {
        _sut = new RedisCacheStore(_cache.Object, NullLogger<RedisCacheStore>.Instance);
    }

    private sealed record Sample(Guid Id, string Name);

    private void SetupGet(byte[]? value) =>
        _cache
            .Setup(c => c.GetAsync(Key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(value);

    private void SetupGetThrows(Exception exception) =>
        _cache
            .Setup(c => c.GetAsync(Key, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

    private void SetupSet(Action<byte[], DistributedCacheEntryOptions>? onCalled = null) =>
        _cache
            .Setup(c => c.SetAsync(
                Key,
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>(
                (_, value, options, _) => onCalled?.Invoke(value, options))
            .Returns(Task.CompletedTask);

    private void SetupSetThrows(Exception exception) =>
        _cache
            .Setup(c => c.SetAsync(
                Key,
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

    [Fact]
    public async Task GetAsync_KeyIsMissing_ReturnsNull()
    {
        SetupGet(null);

        var result = await _sut.GetAsync<Sample>(Key);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_KeyExists_ReturnsDeserializedValue()
    {
        var sample = new Sample(Guid.NewGuid(), SampleName);
        SetupGet(JsonSerializer.SerializeToUtf8Bytes(sample));

        var result = await _sut.GetAsync<Sample>(Key);

        Assert.Equal(sample, result);
    }

    [Fact]
    public async Task GetAsync_StoredValueIsNotValidJson_ReturnsNull()
    {
        SetupGet(Encoding.UTF8.GetBytes(InvalidJson));

        var result = await _sut.GetAsync<Sample>(Key);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_CacheThrows_ReturnsNull()
    {
        SetupGetThrows(new InvalidOperationException(RedisDownMessage));

        var result = await _sut.GetAsync<Sample>(Key);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_OperationCancelled_PropagatesCancellation()
    {
        SetupGetThrows(new OperationCanceledException());

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _sut.GetAsync<Sample>(Key));
    }

    [Fact]
    public async Task SetAsync_ValidValue_StoresJsonWithTtl()
    {
        var sample = new Sample(Guid.NewGuid(), SampleName);
        byte[]? storedBytes = null;
        DistributedCacheEntryOptions? storedOptions = null;
        SetupSet((value, options) =>
        {
            storedBytes = value;
            storedOptions = options;
        });

        await _sut.SetAsync(Key, sample, Ttl);

        Assert.NotNull(storedBytes);
        Assert.Equal(sample, JsonSerializer.Deserialize<Sample>(storedBytes));
        Assert.Equal(Ttl, storedOptions?.AbsoluteExpirationRelativeToNow);
    }

    [Fact]
    public async Task SetAsync_CacheThrows_DoesNotThrow()
    {
        SetupSetThrows(new InvalidOperationException(RedisDownMessage));

        var exception = await Record.ExceptionAsync(
            () => _sut.SetAsync(Key, new Sample(Guid.NewGuid(), SampleName), Ttl));

        Assert.Null(exception);
    }

    [Fact]
    public async Task SetAsync_OperationCancelled_PropagatesCancellation()
    {
        SetupSetThrows(new OperationCanceledException());

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _sut.SetAsync(Key, new Sample(Guid.NewGuid(), SampleName), Ttl));
    }
}
