using System.Text;
using ErrorOr;
using Feedback.API.Common.Errors;
using Feedback.API.Common.Interfaces;
using Feedback.API.Infrastructure.Redis;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Feedback.Tests.Unit.Services.Tests;

public sealed class CachedIdeaServiceTests
{
    private readonly Mock<IIdeaService> _inner = new(MockBehavior.Strict);
    private readonly Mock<IDistributedCache> _cache = new(MockBehavior.Strict);
    private readonly CachedIdeaService _sut;

    public CachedIdeaServiceTests()
    {
        _sut = new CachedIdeaService(_inner.Object, _cache.Object, NullLogger<CachedIdeaService>.Instance);
    }

    private void SetupCacheGet(string? value) =>
        _cache
            .Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(value is null ? null : Encoding.UTF8.GetBytes(value));

    private void SetupInner(Guid ideaId, bool includeOnlyActive, ErrorOr<Guid> result) =>
        _inner
            .Setup(s => s.GetIdeaCityAsync(ideaId, includeOnlyActive, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

    private void SetupCacheSet(Action<string, byte[], DistributedCacheEntryOptions>? onCalled = null) =>
        _cache
            .Setup(c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>(
                (key, value, options, _) => onCalled?.Invoke(key, value, options))
            .Returns(Task.CompletedTask);

    private void VerifyNothingStored() =>
        _cache.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Never);

    [Fact]
    public async Task GetIdeaCityAsync_CacheHit_ReturnsCachedCityWithoutCallingInner()
    {
        var cityId = Guid.NewGuid();
        SetupCacheGet(cityId.ToString());

        var result = await _sut.GetIdeaCityAsync(Guid.NewGuid(), true);

        Assert.False(result.IsError);
        Assert.Equal(cityId, result.Value);
        _inner.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetIdeaCityAsync_CacheMiss_CallsInnerAndStoresCityUnderIdeaKey(bool includeOnlyActive)
    {
        var ideaId = Guid.NewGuid();
        var cityId = Guid.NewGuid();
        string? storedKey = null;
        string? storedValue = null;
        DistributedCacheEntryOptions? storedOptions = null;
        SetupCacheGet(null);
        SetupInner(ideaId, includeOnlyActive, cityId);
        SetupCacheSet((key, value, options) =>
        {
            storedKey = key;
            storedValue = Encoding.UTF8.GetString(value);
            storedOptions = options;
        });

        var result = await _sut.GetIdeaCityAsync(ideaId, includeOnlyActive);

        Assert.Equal(cityId, result.Value);
        Assert.Equal($"idea-city:{ideaId}:{includeOnlyActive}", storedKey);
        Assert.Equal(cityId.ToString(), storedValue);
        Assert.Equal(TimeSpan.FromMinutes(2), storedOptions?.AbsoluteExpirationRelativeToNow);
    }

    [Fact]
    public async Task GetIdeaCityAsync_IdeaNotFound_ReturnsErrorAndDoesNotCacheIt()
    {
        var ideaId = Guid.NewGuid();
        SetupCacheGet(null);
        SetupInner(ideaId, true, FeedbackErrors.IdeaNotFound);

        var result = await _sut.GetIdeaCityAsync(ideaId, true);

        Assert.True(result.IsError);
        Assert.Equal(FeedbackErrors.IdeaNotFound, result.FirstError);
        VerifyNothingStored();
    }

    [Fact]
    public async Task GetIdeaCityAsync_IdeasServiceUnavailable_ReturnsErrorAndDoesNotCacheIt()
    {
        var ideaId = Guid.NewGuid();
        SetupCacheGet(null);
        SetupInner(ideaId, true, FeedbackErrors.IdeasServiceUnavailable);

        var result = await _sut.GetIdeaCityAsync(ideaId, true);

        Assert.True(result.IsError);
        Assert.Equal(FeedbackErrors.IdeasServiceUnavailable, result.FirstError);
        VerifyNothingStored();
    }

    [Fact]
    public async Task GetIdeaCityAsync_CacheReadFails_FallsBackToInner()
    {
        var ideaId = Guid.NewGuid();
        var cityId = Guid.NewGuid();
        _cache
            .Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("redis down"));
        SetupInner(ideaId, true, cityId);
        SetupCacheSet();

        var result = await _sut.GetIdeaCityAsync(ideaId, true);

        Assert.False(result.IsError);
        Assert.Equal(cityId, result.Value);
    }

    [Fact]
    public async Task GetIdeaCityAsync_CacheWriteFails_StillReturnsInnerResult()
    {
        var ideaId = Guid.NewGuid();
        var cityId = Guid.NewGuid();
        SetupCacheGet(null);
        SetupInner(ideaId, true, cityId);
        _cache
            .Setup(c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("redis down"));

        var result = await _sut.GetIdeaCityAsync(ideaId, true);

        Assert.False(result.IsError);
        Assert.Equal(cityId, result.Value);
    }

    [Fact]
    public async Task GetIdeaCityAsync_CacheReadCancelled_PropagatesCancellation()
    {
        _cache
            .Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _sut.GetIdeaCityAsync(Guid.NewGuid(), true));
    }
}
