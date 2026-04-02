using AutoMapper;
using MediatR;
using Polls.Application.Cities.DTOs;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Cities;
using Polls.Domain.Cities.Enums;
using Polls.Domain.Common;

namespace Polls.Application.Cities.Queries.GetCityById;

public sealed class GetCityByIdQueryHandler(
    IUnitOfWork unitOfWork, 
    IMapper mapper)
    : IRequestHandler<GetCityByIdQuery, Result<CityDto>>
{
    public async Task<Result<CityDto>> Handle(
        GetCityByIdQuery query,
        CancellationToken cancellationToken)
    {
        var city = await unitOfWork.Cities.GetByIdAsync(query.Id, cancellationToken);
        if (city is null || (city.Status != CityStatus.Active && query.IncludeOnlyActive))
            return CityErrors.NotFound(query.Id);

        return mapper.Map<CityDto>(city);
    }
}
