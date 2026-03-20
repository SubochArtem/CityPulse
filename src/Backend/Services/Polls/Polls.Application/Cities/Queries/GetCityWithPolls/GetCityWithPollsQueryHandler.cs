using AutoMapper;
using MediatR;
using Polls.Application.Cities.DTOs;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Cities;
using Polls.Domain.Common;

namespace Polls.Application.Cities.Queries.GetCityWithPolls;

public sealed class GetCityWithPollsQueryHandler(
    IUnitOfWork unitOfWork, 
    IMapper mapper)
    : IRequestHandler<GetCityWithPollsQuery, Result<CityWithPollsDto>>
{
    public async Task<Result<CityWithPollsDto>> Handle(
        GetCityWithPollsQuery query,
        CancellationToken cancellationToken)
    {
        var city = await unitOfWork.Cities.GetWithPollsAsync(query.Id, cancellationToken);
        if (city is null)
            return CityErrors.NotFound(query.Id);

        return mapper.Map<CityWithPollsDto>(city);
    }
}
