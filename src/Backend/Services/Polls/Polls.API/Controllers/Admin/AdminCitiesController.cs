using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
using Polls.Domain.Authorization;
using Polls.Domain.Cities.Enums;
using Polls.Domain.Common;

namespace Polls.API.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/cities")]
[Authorize]
public class AdminCitiesController(ISender sender) : ControllerBase
{
    [HttpGet("{id:guid}/polls")]
    [Authorize(Policy = Permissions.Cities.ReadAny)]
    public async Task<Result<CityWithPollsDto>> GetCityWithPolls(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new GetCityWithPollsQuery(
            Id: id,
            IncludeOnlyActive: false);
        
        return await sender.Send(command, cancellationToken);
    }

    [HttpGet]
    [Authorize(Policy = Permissions.Cities.ReadAny)]
    public async Task<Result<PagedList<CityDto>>> GetCities(
        [FromQuery] CityFilter filter,
        CancellationToken cancellationToken)
    {
        var command = new GetCitiesQuery(
            Filter: filter,
            IncludeOnlyActive: false);
        
        return await sender.Send(command, cancellationToken);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.Cities.ReadAny)]
    public async Task<Result<CityDto>> GetCityById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new GetCityByIdQuery(
            Id: id,
            IncludeOnlyActive: false);
        
        return await sender.Send(command, cancellationToken);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.Cities.CreateAny)]
    public async Task<Result<CityDto>> CreateCity(
        CreateCityRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCityCommand(
            Title: request.Title,
            Coordinates: request.Coordinates,
            Description: request.Description);
        
        return await sender.Send(command, cancellationToken);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.Cities.UpdateAny)]
    public async Task<Result<CityDto>> UpdateCity(
        Guid id,
        UpdateCityRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCityCommand(
            Id: id,
            Title: request.Title,
            Coordinates: request.Coordinates,
            Description: request.Description);
        
        return await sender.Send(command, cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.Cities.DeleteAny)]
    public async Task<Result<Unit>> DeleteCity(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteCityCommand(
            Id: id);

        return await sender.Send(command, cancellationToken);
    }

    [HttpPost("{id:guid}/status")]
    [Authorize(Policy = Permissions.Cities.ChangeStatusAny)]
    public async Task<Result<Unit>> ChangeStatus(
        Guid id,
        [FromBody] CityStatus newStatus,
        CancellationToken cancellationToken)
    {
        var command = new ChangeCityStatusCommand(
            Id: id,
            NewStatus: newStatus);
        
        return await sender.Send(command, cancellationToken);
    }
}
