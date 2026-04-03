using AutoMapper;
using MediatR;
using Polls.Application.Common.Interfaces;
using Polls.Application.Ideas.DTOs;
using Polls.Domain.Common;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;

namespace Polls.Application.Ideas.Queries.GetIdeaById;

public sealed class GetIdeaByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<GetIdeaByIdQuery, Result<IdeaDto>>
{
    public async Task<Result<IdeaDto>> Handle(
        GetIdeaByIdQuery query,
        CancellationToken cancellationToken)
    {
        var idea = await unitOfWork.Ideas.GetByIdAsync(query.Id, cancellationToken);
        if (idea is null || (idea.Status != IdeaStatus.Active && query.IncludeOnlyActive))
            return IdeaErrors.NotFound(query.Id);

        return mapper.Map<IdeaDto>(idea);
    }
}
