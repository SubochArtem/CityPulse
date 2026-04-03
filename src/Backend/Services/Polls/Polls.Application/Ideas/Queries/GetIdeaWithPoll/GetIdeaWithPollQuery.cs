using MediatR;
using Polls.Application.Ideas.DTOs;
using Polls.Domain.Common;

namespace Polls.Application.Ideas.Queries.GetIdeaWithPoll;

public sealed record GetIdeaWithPollQuery(Guid Id, bool IncludeOnlyActive = true) : IRequest<Result<IdeaWithPollDto>>;
