using Polls.Application.Common.CQRS;
using Polls.Application.Polls.DTOs;

namespace Polls.Application.Polls.Queries.GetPollWithIdeas;

public record GetPollWithIdeasQuery(Guid Id) : IQuery<PollWithIdeasDto>;
