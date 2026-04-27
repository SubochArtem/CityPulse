using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Polls.API.Common.Extensions;
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
using Polls.Domain.Common;

namespace Polls.API.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/cities")]
[Authorize]
public class AdminCitiesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.Cities.ReadAny)]
    public async Task<Result<PagedList<CityDto>>> GetCities(
        [FromQuery] CityFilter filter,
        CancellationToken cancellationToken)
    {
        var query = new GetCitiesQuery(
            Filter: filter,
            IncludeOnlyActive: false);
        
        return await sender.Send(query, cancellationToken);
    }
    
    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.Cities.ReadAny)]
    public async Task<Result<CityDto>> GetCityById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetCityByIdQuery(
            Id: id,
            IncludeOnlyActive: false);
        
        return await sender.Send(query, cancellationToken);
    }
    
    [HttpGet("{id:guid}/polls")]
    [Authorize(Policy = Permissions.Cities.ReadAny)]
    public async Task<Result<CityWithPollsDto>> GetCityWithPolls(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetCityWithPollsQuery(
            Id: id,
            IncludeOnlyActive: false);
        
        return await sender.Send(query, cancellationToken);
    }
    
    [HttpPost]
    [Authorize(Policy = Permissions.Cities.CreateAny)]
    public async Task<Result<CityDto>> CreateCity(
        [FromForm] CreateCityRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCityCommand(
            Title: request.Title,
            Coordinates: request.Coordinates,
            Description: request.Description,
            Images: request.Images.ToImageFiles()); 
        
        return await sender.Send(command, cancellationToken);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.Cities.UpdateAny)]
    public async Task<Result<CityDto>> UpdateCity(
        Guid id,
        [FromForm] UpdateCityRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCityCommand(
            Id: id,
            Title: request.Title,
            Coordinates: request.Coordinates,
            Description: request.Description,
            ImagesToAdd: request.ImagesToAdd.ToImageFiles(), 
            ImagesToDelete: request.ImagesToDelete);
        
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

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = Permissions.Cities.ChangeStatusAny)]
    public async Task<Result<Unit>> ChangeStatus(
        Guid id,
        ChangeCityStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangeCityStatusCommand(
            Id: id,
            NewStatus: request.NewStatus);
        
        return await sender.Send(command, cancellationToken);
    }
}
