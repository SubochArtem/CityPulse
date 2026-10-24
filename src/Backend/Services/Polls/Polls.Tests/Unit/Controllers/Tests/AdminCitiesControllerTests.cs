using CityPulse.Contracts.Querying.Pagination;
using Polls.API.Controllers.Admin;
using Polls.API.Requests.Cities;
using Polls.Application.Cities.Commands.ChangeStatus;
using Polls.Application.Cities.Commands.CreateCity;
using Polls.Application.Cities.Commands.DeleteCity;
using Polls.Application.Cities.Commands.UpdateCity;
using Polls.Application.Cities.DTOs;
using Polls.Application.Cities.Queries.GetCities;
using Polls.Application.Cities.Queries.GetCityById;
using Polls.Application.Cities.Queries.GetCityWithPolls;
using Polls.Application.Common.Models;
using Polls.Domain.Cities.Enums;
using Polls.Domain.Common;
using Polls.Tests.Infrastructure;
using Xunit;
using MediatRUnit = MediatR.Unit;

namespace Polls.Tests.Unit.Controllers.Tests;

public sealed class AdminCitiesControllerTests : ControllerTestsBase
{
    private const string Title = "Warsaw";
    private const string Description = "Capital of Poland";
    private const string FirstFile = "first.jpg";

    private static readonly Error Failure = Error.Failure("failure");
    private static readonly CoordinatesDto Coordinates = new(52.23, 21.01);

    private readonly AdminCitiesController _sut;

    public AdminCitiesControllerTests()
    {
        _sut = WithUser(new AdminCitiesController(Sender.Object), UserId, UserCityId);
    }

    [Fact]
    public async Task GetCities_Always_QueriesCitiesOfAnyStatusWithSameFilterAndReturnsResult()
    {
        var filter = new CityFilter();
        var expected = Result<PagedList<CityDto>>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        SetupSend<GetCitiesQuery, PagedList<CityDto>>(expected);

        var result = await _sut.GetCities(filter, cts.Token);

        Assert.Same(expected, result);
        var query = SentRequest<GetCitiesQuery>();
        Assert.Same(filter, query.Filter);
        Assert.False(query.IncludeOnlyActive);
        Assert.Equal(cts.Token, SentToken<GetCitiesQuery>());
    }

    [Fact]
    public async Task GetCityById_Always_QueriesCityOfAnyStatusAndReturnsResult()
    {
        var id = Guid.NewGuid();
        var expected = Result<CityDto>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        SetupSend<GetCityByIdQuery, CityDto>(expected);

        var result = await _sut.GetCityById(id, cts.Token);

        Assert.Same(expected, result);
        var query = SentRequest<GetCityByIdQuery>();
        Assert.Equal(id, query.Id);
        Assert.False(query.IncludeOnlyActive);
        Assert.Equal(cts.Token, SentToken<GetCityByIdQuery>());
    }

    [Fact]
    public async Task GetCityWithPolls_Always_QueriesCityOfAnyStatusAndReturnsResult()
    {
        var id = Guid.NewGuid();
        var expected = Result<CityWithPollsDto>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        SetupSend<GetCityWithPollsQuery, CityWithPollsDto>(expected);

        var result = await _sut.GetCityWithPolls(id, cts.Token);

        Assert.Same(expected, result);
        var query = SentRequest<GetCityWithPollsQuery>();
        Assert.Equal(id, query.Id);
        Assert.False(query.IncludeOnlyActive);
        Assert.Equal(cts.Token, SentToken<GetCityWithPollsQuery>());
    }

    [Fact]
    public async Task CreateCity_Always_SendsCommandBuiltFromRequest()
    {
        var expected = Result<CityDto>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        var request = new CreateCityRequest
        {
            Title = Title,
            Coordinates = Coordinates,
            Description = Description,
            Images = FormFiles(FirstFile)
        };
        SetupSend<CreateCityCommand, CityDto>(expected);

        var result = await _sut.CreateCity(request, cts.Token);

        Assert.Same(expected, result);
        var command = SentRequest<CreateCityCommand>();
        Assert.Equal(Title, command.Title);
        Assert.Equal(Coordinates, command.Coordinates);
        Assert.Equal(Description, command.Description);
        Assert.Equal([FirstFile], command.Images!.Select(image => image.FileName));
        Assert.Equal(cts.Token, SentToken<CreateCityCommand>());
    }

    [Fact]
    public async Task UpdateCity_Always_SendsCommandBuiltFromRequest()
    {
        var id = Guid.NewGuid();
        var imageToDelete = Guid.NewGuid();
        var expected = Result<CityDto>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        var request = new UpdateCityRequest
        {
            Title = Title,
            Coordinates = Coordinates,
            Description = Description,
            ImagesToAdd = FormFiles(FirstFile),
            ImagesToDelete = [imageToDelete]
        };
        SetupSend<UpdateCityCommand, CityDto>(expected);

        var result = await _sut.UpdateCity(id, request, cts.Token);

        Assert.Same(expected, result);
        var command = SentRequest<UpdateCityCommand>();
        Assert.Equal(id, command.Id);
        Assert.Equal(Title, command.Title);
        Assert.Equal(Coordinates, command.Coordinates);
        Assert.Equal(Description, command.Description);
        Assert.Equal([FirstFile], command.ImagesToAdd!.Select(image => image.FileName));
        Assert.Equal([imageToDelete], command.ImagesToDelete);
        Assert.Equal(cts.Token, SentToken<UpdateCityCommand>());
    }

    [Fact]
    public async Task DeleteCity_Always_SendsCommandWithCityId()
    {
        var id = Guid.NewGuid();
        var expected = Result<MediatRUnit>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        SetupSend<DeleteCityCommand, MediatRUnit>(expected);

        var result = await _sut.DeleteCity(id, cts.Token);

        Assert.Same(expected, result);
        Assert.Equal(id, SentRequest<DeleteCityCommand>().Id);
        Assert.Equal(cts.Token, SentToken<DeleteCityCommand>());
    }

    [Theory]
    [InlineData(CityStatus.Undefined)]
    [InlineData(CityStatus.Active)]
    [InlineData(CityStatus.Inactive)]
    public async Task ChangeStatus_Always_SendsCommandWithRequestedStatus(CityStatus status)
    {
        var id = Guid.NewGuid();
        var expected = Result<MediatRUnit>.Failure(Failure);
        using var cts = new CancellationTokenSource();
        SetupSend<ChangeCityStatusCommand, MediatRUnit>(expected);

        var result = await _sut.ChangeStatus(id, new ChangeCityStatusRequest { NewStatus = status }, cts.Token);

        Assert.Same(expected, result);
        var command = SentRequest<ChangeCityStatusCommand>();
        Assert.Equal(id, command.Id);
        Assert.Equal(status, command.NewStatus);
        Assert.Equal(cts.Token, SentToken<ChangeCityStatusCommand>());
    }
}
