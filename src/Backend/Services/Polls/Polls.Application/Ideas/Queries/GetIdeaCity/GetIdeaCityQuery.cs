using Polls.Application.Common.CQRS;
using Polls.Application.Ideas.DTOs;

namespace Polls.Application.Ideas.Queries.GetIdeaCity;

public record GetIdeaCityQuery(Guid Id, bool IncludeOnlyActive = true) : IQuery<IdeaCityDto>;
