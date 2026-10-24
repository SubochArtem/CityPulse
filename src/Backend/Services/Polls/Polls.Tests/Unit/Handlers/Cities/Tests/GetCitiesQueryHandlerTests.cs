using CityPulse.Contracts.Querying.Pagination;
using Moq;
using Polls.Application.Cities.Queries.GetCities;
using Polls.Application.Common.Models;
using Polls.Domain.Cities;
using Polls.Domain.Cities.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.TestData;
using Polls.Tests.Unit.Handlers.Cities.TestData;
using Xunit;

namespace Polls.Tests.Unit.Handlers.Cities.Tests;

public sealed class GetCitiesQueryHandlerTests : HandlerTestsBase
{
    private const int Page = 2;
    private const int PageSize = 3;
    private const int TotalCount = 8;

    private readonly GetCitiesQueryHandler _sut;

    public GetCitiesQueryHandlerTests()
    {
        _sut = new GetCitiesQueryHandler(UnitOfWork.Object, Mapper);
    }

    private void SetupRepository(List<City>? cities = null) =>
        UnitOfWork.Cities
            .Setup(x => x.GetFilteredAsync(It.IsAny<CityFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<City>(cities ?? [], Page, PageSize, TotalCount));

    [Theory]
    [MemberData(nameof(CityHandlerTestData.NonActiveFilterStatuses), MemberType = typeof(CityHandlerTestData))]
    public async Task Handle_OnlyActiveRequested_ForcesActiveStatusInFilter(CityStatus? requestedStatus)
    {
        var filter = new CityFilter { Status = requestedStatus };
        SetupRepository();

        await _sut.Handle(new GetCitiesQuery(filter, IncludeOnlyActive: true), CancellationToken.None);

        Assert.Equal(CityStatus.Active, filter.Status);
        UnitOfWork.Cities.Verify(
            x => x.GetFilteredAsync(filter, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [MemberData(nameof(CityHandlerTestData.AnyFilterStatuses), MemberType = typeof(CityHandlerTestData))]
    public async Task Handle_AllRequested_KeepsFilterStatusUnchanged(CityStatus? requestedStatus)
    {
        var filter = new CityFilter { Status = requestedStatus };
        SetupRepository();

        await _sut.Handle(new GetCitiesQuery(filter, IncludeOnlyActive: false), CancellationToken.None);

        Assert.Equal(requestedStatus, filter.Status);
    }

    [Fact]
    public async Task Handle_CitiesFound_ReturnsMappedPageKeepingPaging()
    {
        var cities = CityFakers.Cities().Generate(PageSize);
        SetupRepository(cities);

        var result = await _sut.Handle(new GetCitiesQuery(new CityFilter()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(cities.Select(c => c.Id), result.Value!.Items.Select(c => c.Id));
        Assert.Equal(Page, result.Value.Page);
        Assert.Equal(PageSize, result.Value.PageSize);
        Assert.Equal(TotalCount, result.Value.TotalCount);
    }

    [Fact]
    public async Task Handle_NoCitiesFound_ReturnsEmptyItems()
    {
        SetupRepository();

        var result = await _sut.Handle(new GetCitiesQuery(new CityFilter()), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
    }
}
