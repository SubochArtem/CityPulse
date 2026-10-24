using Moq;
using Polls.Application.Cities.Queries.GetCityWithPolls;
using Polls.Domain.Cities;
using Polls.Domain.Cities.Enums;
using Polls.Domain.Polls.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.TestData;
using Polls.Tests.Unit.Handlers.Cities.TestData;
using Xunit;

namespace Polls.Tests.Unit.Handlers.Cities.Tests;

public sealed class GetCityWithPollsQueryHandlerTests : HandlerTestsBase
{
    private const int PollsCount = 2;

    private readonly GetCityWithPollsQueryHandler _sut;

    public GetCityWithPollsQueryHandlerTests()
    {
        _sut = new GetCityWithPollsQueryHandler(UnitOfWork.Object, Mapper);
    }

    private void SetupCity(City? city, Guid id, PollStatus? expectedPollStatus) =>
        UnitOfWork.Cities
            .Setup(x => x.GetWithPollsAsync(id, expectedPollStatus, It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

    private static City BuildCityWithPolls()
    {
        var city = CityFakers.Cities().Generate();
        city.Polls = PollFakers.Polls().Generate(PollsCount);

        return city;
    }

    [Fact]
    public async Task Handle_OnlyActiveRequested_AsksRepositoryForActivePollsOnly()
    {
        var city = BuildCityWithPolls();
        SetupCity(city, city.Id, PollStatus.Active);

        await _sut.Handle(new GetCityWithPollsQuery(city.Id, IncludeOnlyActive: true), CancellationToken.None);

        UnitOfWork.Cities.Verify(
            x => x.GetWithPollsAsync(city.Id, PollStatus.Active, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_AllRequested_AsksRepositoryForPollsOfAnyStatus()
    {
        var city = BuildCityWithPolls();
        SetupCity(city, city.Id, null);

        await _sut.Handle(new GetCityWithPollsQuery(city.Id, IncludeOnlyActive: false), CancellationToken.None);

        UnitOfWork.Cities.Verify(
            x => x.GetWithPollsAsync(city.Id, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CityFound_ReturnsMappedCityWithPolls()
    {
        var city = BuildCityWithPolls();
        SetupCity(city, city.Id, PollStatus.Active);

        var result = await _sut.Handle(new GetCityWithPollsQuery(city.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(city.Id, result.Value!.Id);
        Assert.Equal(city.Polls.Select(p => p.Id), result.Value.Polls.Select(p => p.Id));
    }

    [Fact]
    public async Task Handle_CityDoesNotExist_ReturnsNotFoundError()
    {
        var id = Guid.NewGuid();
        SetupCity(null, id, PollStatus.Active);

        var result = await _sut.Handle(new GetCityWithPollsQuery(id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(CityErrors.NotFound(id), result.Error);
    }

    [Theory]
    [MemberData(nameof(CityHandlerTestData.NotActiveStatuses), MemberType = typeof(CityHandlerTestData))]
    public async Task Handle_CityIsNotActiveAndOnlyActiveRequested_ReturnsNotFoundError(CityStatus status)
    {
        var city = BuildCityWithPolls();
        city.Status = status;
        SetupCity(city, city.Id, PollStatus.Active);

        var result = await _sut.Handle(
            new GetCityWithPollsQuery(city.Id, IncludeOnlyActive: true),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(CityErrors.NotFound(city.Id), result.Error);
    }

    [Theory]
    [MemberData(nameof(CityHandlerTestData.NotActiveStatuses), MemberType = typeof(CityHandlerTestData))]
    public async Task Handle_CityIsNotActiveAndAllRequested_ReturnsCityWithPolls(CityStatus status)
    {
        var city = BuildCityWithPolls();
        city.Status = status;
        SetupCity(city, city.Id, null);

        var result = await _sut.Handle(
            new GetCityWithPollsQuery(city.Id, IncludeOnlyActive: false),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal((int)status, result.Value!.Status);
        Assert.Equal(PollsCount, result.Value.Polls.Count);
    }
}
