using CityPulse.Common.Api.Caching;
using ErrorOr;
using Feedback.API.Common.Errors;
using Feedback.API.Common.Interfaces;
using Feedback.API.Infrastructure.Caching;
using Moq;
using Xunit;

namespace Feedback.Tests.Unit.Services.Tests;

public sealed class CachedIdeaServiceTests
{
    private const string CacheKeyPrefix = "idea-city:";

    private static readonly TimeSpan ExpectedTtl = TimeSpan.FromMinutes(2);

    private readonly Mock<IIdeaService> _inner = new(MockBehavior.Strict);
    private readonly Mock<ICacheStore> _cache = new(MockBehavior.Strict);
    private readonly CachedIdeaService _sut;

    public CachedIdeaServiceTests()
    {
        _sut = new CachedIdeaService(_inner.Object, _cache.Object);
    }

    private static string KeyFor(Guid ideaId, bool includeOnlyActive) =>
        $"{CacheKeyPrefix}{ideaId}:{includeOnlyActive}";

    private void SetupCacheGet(
        Guid ideaId,
        bool includeOnlyActive,
        IdeaCityCache? cached,
        Action<CancellationToken>? onCalled = null) =>
        _cache
            .Setup(c => c.GetAsync<IdeaCityCache>(KeyFor(ideaId, includeOnlyActive), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((_, token) => onCalled?.Invoke(token))
            .ReturnsAsync(cached);

    private void SetupCacheSet(
        Guid ideaId,
        bool includeOnlyActive,
        Action<IdeaCityCache, TimeSpan, CancellationToken>? onCalled = null) =>
        _cache
            .Setup(c => c.SetAsync(
                KeyFor(ideaId, includeOnlyActive),
                It.IsAny<IdeaCityCache>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, IdeaCityCache, TimeSpan, CancellationToken>(
                (_, value, ttl, token) => onCalled?.Invoke(value, ttl, token))
            .Returns(Task.CompletedTask);

    private void SetupInner(
        Guid ideaId,
        bool includeOnlyActive,
        ErrorOr<Guid> result,
        Action<CancellationToken>? onCalled = null) =>
        _inner
            .Setup(s => s.GetIdeaCityAsync(ideaId, includeOnlyActive, It.IsAny<CancellationToken>()))
            .Callback<Guid, bool, CancellationToken>((_, _, token) => onCalled?.Invoke(token))
            .ReturnsAsync(result);

    private void VerifyNothingStored() =>
        _cache.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<IdeaCityCache>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()), Times.Never);

    [Fact]
    public async Task GetIdeaCityAsync_CacheHit_ReturnsCachedCityWithoutCallingInner()
    {
        var ideaId = Guid.NewGuid();
        var cityId = Guid.NewGuid();
        SetupCacheGet(ideaId, true, new IdeaCityCache(cityId));

        var result = await _sut.GetIdeaCityAsync(ideaId, true);

        Assert.False(result.IsError);
        Assert.Equal(cityId, result.Value);
        _inner.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetIdeaCityAsync_CacheMiss_ReturnsCityFromInner(bool includeOnlyActive)
    {
        var ideaId = Guid.NewGuid();
        var cityId = Guid.NewGuid();
        SetupCacheGet(ideaId, includeOnlyActive, null);
        SetupInner(ideaId, includeOnlyActive, cityId);
        SetupCacheSet(ideaId, includeOnlyActive);

        var result = await _sut.GetIdeaCityAsync(ideaId, includeOnlyActive);

        Assert.False(result.IsError);
        Assert.Equal(cityId, result.Value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetIdeaCityAsync_CacheMiss_StoresCityUnderIdeaKeyWithTtl(bool includeOnlyActive)
    {
        var ideaId = Guid.NewGuid();
        var cityId = Guid.NewGuid();
        IdeaCityCache? storedValue = null;
        TimeSpan? storedTtl = null;
        SetupCacheGet(ideaId, includeOnlyActive, null);
        SetupInner(ideaId, includeOnlyActive, cityId);
        SetupCacheSet(ideaId, includeOnlyActive, (value, ttl, _) =>
        {
            storedValue = value;
            storedTtl = ttl;
        });

        await _sut.GetIdeaCityAsync(ideaId, includeOnlyActive);

        Assert.Equal(new IdeaCityCache(cityId), storedValue);
        Assert.Equal(ExpectedTtl, storedTtl);
    }

    [Fact]
    public async Task GetIdeaCityAsync_CacheMiss_PassesCancellationTokenToInnerAndCache()
    {
        var ideaId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();
        CancellationToken? getToken = null;
        CancellationToken? innerToken = null;
        CancellationToken? setToken = null;
        SetupCacheGet(ideaId, true, null, token => getToken = token);
        SetupInner(ideaId, true, Guid.NewGuid(), token => innerToken = token);
        SetupCacheSet(ideaId, true, (_, _, token) => setToken = token);

        await _sut.GetIdeaCityAsync(ideaId, true, cts.Token);

        Assert.Equal(cts.Token, getToken);
        Assert.Equal(cts.Token, innerToken);
        Assert.Equal(cts.Token, setToken);
    }

    [Fact]
    public async Task GetIdeaCityAsync_IdeaNotFound_ReturnsErrorAndDoesNotCacheIt()
    {
        var ideaId = Guid.NewGuid();
        SetupCacheGet(ideaId, true, null);
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
        SetupCacheGet(ideaId, true, null);
        SetupInner(ideaId, true, FeedbackErrors.IdeasServiceUnavailable);

        var result = await _sut.GetIdeaCityAsync(ideaId, true);

        Assert.True(result.IsError);
        Assert.Equal(FeedbackErrors.IdeasServiceUnavailable, result.FirstError);
        VerifyNothingStored();
    }
}
