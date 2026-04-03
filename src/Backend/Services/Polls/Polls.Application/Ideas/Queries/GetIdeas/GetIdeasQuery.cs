using Polls.Application.Common.CQRS;
using Polls.Application.Common.Models;
using Polls.Application.Ideas.DTOs;

namespace Polls.Application.Ideas.Queries.GetIdeas;

public record GetIdeasQuery(IdeaFilter Filter, bool IncludeOnlyActive = true) : IQuery<PagedList<IdeaDto>>;
