using Polls.Application.Common.CQRS;
using Polls.Application.Common.Models;
using Polls.Application.Polls.DTOs;

namespace Polls.Application.Polls.Queries.GetPolls;

public record GetPollsQuery(PollFilter Filter, bool IncludeOnlyActive = true) : IQuery<PagedList<PollDto>>;
