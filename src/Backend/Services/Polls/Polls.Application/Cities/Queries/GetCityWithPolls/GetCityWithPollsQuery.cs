using Polls.Application.Cities.DTOs;
using Polls.Application.Common.CQRS;

namespace Polls.Application.Cities.Queries.GetCityWithPolls;

public record GetCityWithPollsQuery(Guid Id) : IQuery<CityWithPollsDto>;
