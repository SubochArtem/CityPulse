using AutoMapper;
using MediatR;
using Polls.Application.Common.Interfaces;
using Polls.Application.Ideas.DTOs;
using Polls.Domain.Common;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;

namespace Polls.Application.Ideas.Queries.GetIdeaWithPoll;

public sealed class GetIdeaWithPollQueryHandler(
    IUnitOfWork unitOfWork, 
    IMapper mapper)
    : IRequestHandler<GetIdeaWithPollQuery, Result<IdeaWithPollDto>>
{
    public async Task<Result<IdeaWithPollDto>> Handle(
        GetIdeaWithPollQuery query,
        CancellationToken cancellationToken)
    {
        var idea = await unitOfWork.Ideas.GetWithPollAsync(query.Id, cancellationToken);

        if (idea is null || (idea.Status != IdeaStatus.Active && query.IncludeOnlyActive))
            return IdeaErrors.NotFound(query.Id);

        return mapper.Map<IdeaWithPollDto>(idea);
    }
}
