using CityPulse.Common.Api.Caching;
using Grpc.Core;
using Moq;
using Users.DataAccess.DTOs;
using Users.DataAccess.Interfaces;
using Users.DataAccess.Services;
using Xunit;

namespace Users.Tests.Unit.Services.Tests;

public sealed class CachedCityServiceTests
{
    private const string CityName = "Warsaw";
    private const string CacheKeyPrefix = "city:";
    private const string FailureMessage = "city service failure";

    private static readonly TimeSpan ExpectedTtl = TimeSpan.FromMinutes(1);

    private readonly Mock<ICityService> _inner = new(MockBehavior.Strict);
    private readonly Mock<ICacheStore> _cache = new(MockBehavior.Strict);
    private readonly CachedCityService _sut;

    public CachedCityServiceTests()
    {
        _sut = new CachedCityService(_inner.Object, _cache.Object);
    }

    private static CityDto BuildCity(Guid cityId) => new()
    {
        Id = cityId,
        Name = CityName,
        Status = CityStatus.Active
    };

    private static string KeyFor(Guid cityId) => $"{CacheKeyPrefix}{cityId}";

    private void SetupCacheGet(Guid cityId, CityDto? cached, Action<CancellationToken>? onCalled = null) =>
        _cache
            .Setup(c => c.GetAsync<CityDto>(KeyFor(cityId), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((_, token) => onCalled?.Invoke(token))
            .ReturnsAsync(cached);

    private void SetupCacheSet(Guid cityId, Action<CityDto, TimeSpan, CancellationToken>? onCalled = null) =>
        _cache
            .Setup(c => c.SetAsync(
                KeyFor(cityId),
                It.IsAny<CityDto>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, CityDto, TimeSpan, CancellationToken>(
                (_, city, ttl, token) => onCalled?.Invoke(city, ttl, token))
            .Returns(Task.CompletedTask);

    private void SetupInner(Guid cityId, CityDto city, Action<CancellationToken>? onCalled = null) =>
        _inner
            .Setup(s => s.GetCityAsync(cityId, It.IsAny<CancellationToken>()))
            .Callback<Guid, CancellationToken>((_, token) => onCalled?.Invoke(token))
            .ReturnsAsync(city);

    [Fact]
    public async Task GetCityAsync_CacheHit_ReturnsCachedCityWithoutCallingInner()
    {
        var cityId = Guid.NewGuid();
        var cached = BuildCity(cityId);
        SetupCacheGet(cityId, cached);

        var result = await _sut.GetCityAsync(cityId);

        Assert.Same(cached, result);
        _inner.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetCityAsync_CacheMiss_ReturnsCityFromInner()
    {
        var cityId = Guid.NewGuid();
        var city = BuildCity(cityId);
        SetupCacheGet(cityId, null);
        SetupInner(cityId, city);
        SetupCacheSet(cityId);

        var result = await _sut.GetCityAsync(cityId);

        Assert.Same(city, result);
    }

    [Fact]
    public async Task GetCityAsync_CacheMiss_StoresCityWithOneMinuteTtl()
    {
        var cityId = Guid.NewGuid();
        var city = BuildCity(cityId);
        CityDto? storedCity = null;
        TimeSpan? storedTtl = null;
        SetupCacheGet(cityId, null);
        SetupInner(cityId, city);
        SetupCacheSet(cityId, (stored, ttl, _) =>
        {
            storedCity = stored;
            storedTtl = ttl;
        });

        await _sut.GetCityAsync(cityId);

        Assert.Same(city, storedCity);
        Assert.Equal(ExpectedTtl, storedTtl);
    }

    [Fact]
    public async Task GetCityAsync_CacheMiss_PassesCancellationTokenToInnerAndCache()
    {
        var cityId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();
        CancellationToken? getToken = null;
        CancellationToken? innerToken = null;
        CancellationToken? setToken = null;
        SetupCacheGet(cityId, null, token => getToken = token);
        SetupInner(cityId, BuildCity(cityId), token => innerToken = token);
        SetupCacheSet(cityId, (_, _, token) => setToken = token);

        await _sut.GetCityAsync(cityId, cts.Token);

        Assert.Equal(cts.Token, getToken);
        Assert.Equal(cts.Token, innerToken);
        Assert.Equal(cts.Token, setToken);
    }

    [Fact]
    public async Task GetCityAsync_InnerThrows_PropagatesExceptionAndDoesNotStore()
    {
        var cityId = Guid.NewGuid();
        var exception = new RpcException(new Status(StatusCode.NotFound, FailureMessage));
        SetupCacheGet(cityId, null);
        _inner
            .Setup(s => s.GetCityAsync(cityId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var thrown = await Assert.ThrowsAsync<RpcException>(() => _sut.GetCityAsync(cityId));

        Assert.Same(exception, thrown);
        _cache.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<CityDto>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
