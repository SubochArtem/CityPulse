using Polls.Application.Common.CQRS;
using Polls.Application.Ideas.DTOs;

namespace Polls.Application.Ideas.Queries.GetIdeaById;

public record GetIdeaByIdQuery(Guid Id, bool IncludeOnlyActive = true) : IQuery<IdeaDto>;
