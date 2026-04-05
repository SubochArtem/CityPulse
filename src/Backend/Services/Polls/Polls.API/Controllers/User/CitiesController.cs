using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Polls.Application.Cities.DTOs;
using Polls.Application.Cities.Queries.GetCities;
using Polls.Application.Cities.Queries.GetCityById;
using Polls.Application.Cities.Queries.GetCityWithPolls;
using Polls.Application.Common.Models;
using Polls.Domain.Common;

namespace Polls.API.Controllers.User;

[ApiController]
[Route("api/v1/cities")]
public class CitiesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<Result<PagedList<CityDto>>> GetCities(
        [FromQuery] CityFilter filter,
        CancellationToken cancellationToken)
    {
        var query = new GetCitiesQuery(
            Filter: filter,
            IncludeOnlyActive: true);
        
        return await sender.Send(query, cancellationToken);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<Result<CityDto>> GetCityById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetCityByIdQuery(
            Id: id,
            IncludeOnlyActive: true);
        
        return await sender.Send(query, cancellationToken);
    }

    [HttpGet("{id:guid}/polls")]
    [AllowAnonymous]
    public async Task<Result<CityWithPollsDto>> GetCityWithPolls(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetCityWithPollsQuery(
            Id: id,
            IncludeOnlyActive: true);
        
        return await sender.Send(query, cancellationToken);
    }
}
