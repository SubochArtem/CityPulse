using CityPulse.Contracts.Querying.Pagination;
using Polls.API.Controllers.User;
using Polls.Application.Cities.DTOs;
using Polls.Application.Cities.Queries.GetCities;
using Polls.Application.Cities.Queries.GetCityById;
using Polls.Application.Cities.Queries.GetCityWithPolls;
using Polls.Application.Common.Models;
using Polls.Domain.Common;
using Polls.Tests.Infrastructure;
using Xunit;

namespace Polls.Tests.Unit.Controllers.Tests;

public sealed class CitiesControllerTests : ControllerTestsBase
{
    private const string ErrorDescription = "failure";

    private static readonly Error Failure = Error.Failure(ErrorDescription);

    private readonly CitiesController _sut;

    public CitiesControllerTests()
    {
        _sut = WithUser(new CitiesController(Sender.Object), UserId, UserCityId);
    }

    [Fact]
    public async Task GetCities_Always_QueriesOnlyActiveCitiesWithSameFilterAndReturnsResult()
    {
        var filter = new CityFilter();
        var expected = Result<PagedList<CityDto>>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        SetupSend<GetCitiesQuery, PagedList<CityDto>>(expected);

        var result = await _sut.GetCities(filter, cts.Token);

        Assert.Same(expected, result);
        var query = SentRequest<GetCitiesQuery>();
        Assert.Same(filter, query.Filter);
        Assert.True(query.IncludeOnlyActive);
        Assert.Equal(cts.Token, SentToken<GetCitiesQuery>());
    }

    [Fact]
    public async Task GetCityById_Always_QueriesOnlyActiveCityAndReturnsResult()
    {
        var id = Guid.NewGuid();
        var expected = Result<CityDto>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        SetupSend<GetCityByIdQuery, CityDto>(expected);

        var result = await _sut.GetCityById(id, cts.Token);

        Assert.Same(expected, result);
        var query = SentRequest<GetCityByIdQuery>();
        Assert.Equal(id, query.Id);
        Assert.True(query.IncludeOnlyActive);
        Assert.Equal(cts.Token, SentToken<GetCityByIdQuery>());
    }

    [Fact]
    public async Task GetCityWithPolls_Always_QueriesOnlyActiveCityAndReturnsResult()
    {
        var id = Guid.NewGuid();
        var expected = Result<CityWithPollsDto>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        SetupSend<GetCityWithPollsQuery, CityWithPollsDto>(expected);

        var result = await _sut.GetCityWithPolls(id, cts.Token);

        Assert.Same(expected, result);
        var query = SentRequest<GetCityWithPollsQuery>();
        Assert.Equal(id, query.Id);
        Assert.True(query.IncludeOnlyActive);
        Assert.Equal(cts.Token, SentToken<GetCityWithPollsQuery>());
    }
}
