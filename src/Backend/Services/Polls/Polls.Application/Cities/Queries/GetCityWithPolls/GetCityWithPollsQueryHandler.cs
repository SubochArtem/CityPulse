using AutoMapper;
using MediatR;
using Polls.Application.Cities.DTOs;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Cities;
using Polls.Domain.Cities.Enums;
using Polls.Domain.Common;
using Polls.Domain.Polls.Enums;

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
        PollStatus? pollStatusFilter = query.IncludeOnlyActive 
            ? PollStatus.Active 
            : null;

        var city = await unitOfWork.Cities.GetWithPollsAsync(
            query.Id, 
            pollStatusFilter, 
            cancellationToken);

        if (city is null || (city.Status != CityStatus.Active && query.IncludeOnlyActive))
            return CityErrors.NotFound(query.Id);

        return mapper.Map<CityWithPollsDto>(city);
    }
}
