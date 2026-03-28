using AutoMapper;
using MediatR;
using Polls.Application.Common.Interfaces;
using Polls.Application.Common.Models;
using Polls.Application.Polls.DTOs;
using Polls.Domain.Common;

namespace Polls.Application.Polls.Queries.GetPolls;

public sealed class GetPollsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetPollsQuery, Result<PagedList<PollDto>>>
{
    public async Task<Result<PagedList<PollDto>>> Handle(
        GetPollsQuery query,
        CancellationToken cancellationToken)
    {
        var polls = await unitOfWork.Polls.GetFilteredAsync(query.Filter, cancellationToken);

        return polls.Map(mapper.Map<PollDto>);
    }
}
