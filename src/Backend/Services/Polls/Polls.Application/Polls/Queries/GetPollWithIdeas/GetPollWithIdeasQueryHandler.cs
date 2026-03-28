using AutoMapper;
using MediatR;
using Polls.Application.Common.Interfaces;
using Polls.Application.Polls.DTOs;
using Polls.Domain.Common;
using Polls.Domain.Polls;

namespace Polls.Application.Polls.Queries.GetPollWithIdeas;

public sealed class GetPollWithIdeasQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetPollWithIdeasQuery, Result<PollWithIdeasDto>>
{
    public async Task<Result<PollWithIdeasDto>> Handle(
        GetPollWithIdeasQuery query,
        CancellationToken cancellationToken)
    {
        var poll = await unitOfWork.Polls.GetWithIdeasAsync(query.Id, cancellationToken);
        if (poll is null)
            return PollErrors.NotFound(query.Id);

        return mapper.Map<PollWithIdeasDto>(poll);
    }
}
