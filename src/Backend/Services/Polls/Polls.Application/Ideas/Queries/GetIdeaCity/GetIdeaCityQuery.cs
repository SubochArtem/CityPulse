using Polls.Application.Common.CQRS;

namespace Polls.Application.Ideas.Queries.GetIdeaCity;

public record GetIdeaCityQuery(Guid Id, bool IncludeOnlyActive = true) : IQuery<Guid>;
