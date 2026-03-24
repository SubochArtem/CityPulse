using MediatR;
using Polls.Application.Ideas.DTOs;
using Polls.Domain.Common;

namespace Polls.Application.Ideas.Queries.GetIdeaWithPoll;

public sealed record GetIdeaWithPollQuery(Guid Id) : IRequest<Result<IdeaWithPollDto>>;
