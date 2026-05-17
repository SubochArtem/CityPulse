using AutoMapper;
using MediatR;
using Polls.Application.Common.Interfaces;
using Polls.Application.Polls.DTOs;
using Polls.Domain.Common;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;

namespace Polls.Application.Polls.Queries.GetPollById;

public sealed class GetPollByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetPollByIdQuery, Result<PollDto>>
{
    public async Task<Result<PollDto>> Handle(
        GetPollByIdQuery query,
        CancellationToken cancellationToken)
    {
        var poll = await unitOfWork.Polls.GetByIdWithImagesAsync(query.Id, cancellationToken);
        if (poll is null || (poll.Status != PollStatus.Active && query.IncludeOnlyActive))
            return PollErrors.NotFound(query.Id);

        return mapper.Map<PollDto>(poll);
    }
}
