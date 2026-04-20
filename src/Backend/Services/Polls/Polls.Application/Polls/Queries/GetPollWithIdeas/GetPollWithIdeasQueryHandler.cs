using AutoMapper;
using MediatR;
using Polls.Application.Common.Interfaces;
using Polls.Application.Polls.DTOs;
using Polls.Domain.Common;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;

namespace Polls.Application.Polls.Queries.GetPollWithIdeas;

public sealed class GetPollWithIdeasQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetPollWithIdeasQuery, Result<PollWithIdeasDto>>
{
    public async Task<Result<PollWithIdeasDto>> Handle(
        GetPollWithIdeasQuery query,
        CancellationToken cancellationToken)
    {
        IdeaStatus? ideaStatusFilter = query.IncludeOnlyActive
            ? IdeaStatus.Active
            : null;

        var poll = await unitOfWork.Polls.GetWithIdeasAsync(
            query.Id,
            ideaStatusFilter,
            cancellationToken);

        if (poll is null || (poll.Status != PollStatus.Active && query.IncludeOnlyActive))
            return PollErrors.NotFound(query.Id);

        return mapper.Map<PollWithIdeasDto>(poll);
    }
}
