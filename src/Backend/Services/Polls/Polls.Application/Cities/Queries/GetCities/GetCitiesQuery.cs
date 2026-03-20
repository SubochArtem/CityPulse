using Polls.Application.Cities.DTOs;
using Polls.Application.Common.CQRS;
using Polls.Application.Common.Models;

namespace Polls.Application.Cities.Queries.GetCities;

public record GetCitiesQuery(CityFilter Filter) : IQuery<PagedList<CityDto>>;
