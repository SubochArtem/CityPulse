using AutoMapper;
using MediatR;
using Polls.Application.Common.Interfaces;
using Polls.Application.Common.Models;
using Polls.Application.Ideas.DTOs;
using Polls.Domain.Common;
using Polls.Domain.Ideas.Enums;

namespace Polls.Application.Ideas.Queries.GetIdeas;

public sealed class GetIdeasQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetIdeasQuery, Result<PagedList<IdeaDto>>>
{
    public async Task<Result<PagedList<IdeaDto>>> Handle(
        GetIdeasQuery query,
        CancellationToken cancellationToken)
    {
        if (query.IncludeOnlyActive)
            query.Filter.Status = IdeaStatus.Active;
        
        var ideas = await unitOfWork.Ideas.GetFilteredAsync(query.Filter, cancellationToken);
        return ideas.Map(mapper.Map<IdeaDto>);
    }
}
