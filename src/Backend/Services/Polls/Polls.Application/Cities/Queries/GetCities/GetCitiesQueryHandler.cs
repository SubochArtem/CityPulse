using AutoMapper;
using MediatR;
using Polls.Application.Cities.DTOs;
using Polls.Application.Common.Interfaces;
using Polls.Application.Common.Models;
using Polls.Domain.Common;

namespace Polls.Application.Cities.Queries.GetCities;

public sealed class GetCitiesQueryHandler(
    IUnitOfWork unitOfWork, 
    IMapper mapper)
    : IRequestHandler<GetCitiesQuery, Result<PagedList<CityDto>>>
{
    public async Task<Result<PagedList<CityDto>>> Handle(
        GetCitiesQuery query,
        CancellationToken cancellationToken)
    {
        var cities = await unitOfWork.Cities.GetFilteredAsync(query.Filter, cancellationToken);

        return cities.Map(mapper.Map<CityDto>);
    }
}
