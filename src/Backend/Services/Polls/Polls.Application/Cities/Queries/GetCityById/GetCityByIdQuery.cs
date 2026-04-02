using Polls.Application.Cities.DTOs;
using Polls.Application.Common.CQRS;

namespace Polls.Application.Cities.Queries.GetCityById;

public record GetCityByIdQuery(Guid Id, bool IncludeOnlyActive = true) : IQuery<CityDto>;
