using Moq;
using Polls.Application.Cities.Queries.GetCityById;
using Polls.Domain.Cities;
using Polls.Domain.Cities.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.TestData;
using Polls.Tests.Unit.Handlers.Cities.TestData;
using Xunit;

namespace Polls.Tests.Unit.Handlers.Cities.Tests;

public sealed class GetCityByIdQueryHandlerTests : HandlerTestsBase
{
    private readonly GetCityByIdQueryHandler _sut;

    public GetCityByIdQueryHandlerTests()
    {
        _sut = new GetCityByIdQueryHandler(UnitOfWork.Object, Mapper);
    }

    private void SetupCity(City? city, Guid id) =>
        UnitOfWork.Cities
            .Setup(x => x.GetByIdWithImagesAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

    [Fact]
    public async Task Handle_CityIsActive_ReturnsMappedCity()
    {
        var city = CityFakers.Cities().Generate();
        SetupCity(city, city.Id);

        var result = await _sut.Handle(new GetCityByIdQuery(city.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(city.Id, result.Value!.Id);
        Assert.Equal(city.Title, result.Value.Title);
    }

    [Fact]
    public async Task Handle_CityDoesNotExist_ReturnsNotFoundError()
    {
        var id = Guid.NewGuid();
        SetupCity(null, id);

        var result = await _sut.Handle(new GetCityByIdQuery(id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(CityErrors.NotFound(id), result.Error);
    }

    [Theory]
    [MemberData(nameof(CityHandlerTestData.NotActiveStatuses), MemberType = typeof(CityHandlerTestData))]
    public async Task Handle_CityIsNotActiveAndOnlyActiveRequested_ReturnsNotFoundError(CityStatus status)
    {
        var city = CityFakers.Cities().Generate();
        city.Status = status;
        SetupCity(city, city.Id);

        var result = await _sut.Handle(new GetCityByIdQuery(city.Id, IncludeOnlyActive: true), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(CityErrors.NotFound(city.Id), result.Error);
    }

    [Theory]
    [MemberData(nameof(CityHandlerTestData.NotActiveStatuses), MemberType = typeof(CityHandlerTestData))]
    public async Task Handle_CityIsNotActiveAndAllRequested_ReturnsCity(CityStatus status)
    {
        var city = CityFakers.Cities().Generate();
        city.Status = status;
        SetupCity(city, city.Id);

        var result = await _sut.Handle(new GetCityByIdQuery(city.Id, IncludeOnlyActive: false), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal((int)status, result.Value!.Status);
    }

    [Fact]
    public async Task Handle_CancellationTokenProvided_PassesItToRepository()
    {
        var city = CityFakers.Cities().Generate();
        using var cts = new CancellationTokenSource();
        UnitOfWork.Cities
            .Setup(x => x.GetByIdWithImagesAsync(city.Id, cts.Token))
            .ReturnsAsync(city);

        await _sut.Handle(new GetCityByIdQuery(city.Id), cts.Token);

        UnitOfWork.Cities.Verify(x => x.GetByIdWithImagesAsync(city.Id, cts.Token), Times.Once);
    }
}
