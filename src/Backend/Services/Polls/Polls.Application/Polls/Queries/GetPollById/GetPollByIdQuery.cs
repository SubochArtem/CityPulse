using Polls.Application.Common.CQRS;
using Polls.Application.Polls.DTOs;

namespace Polls.Application.Polls.Queries.GetPollById;

public record GetPollByIdQuery(Guid Id, bool IncludeOnlyActive = true) : IQuery<PollDto>;
