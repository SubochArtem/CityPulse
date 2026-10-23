using MediatR;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Common;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;

namespace Polls.Application.Ideas.Queries.GetIdeaCity;

public sealed class GetIdeaCityQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetIdeaCityQuery, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        GetIdeaCityQuery query,
        CancellationToken cancellationToken)
    {
        var idea = await unitOfWork.Ideas.GetWithPollAsync(query.Id, cancellationToken);
        if (idea is null || (idea.AccessStatus != IdeaAccessStatus.Active && query.IncludeOnlyActive))
            return IdeaErrors.NotFound(query.Id);

        return idea.Poll.CityId;
    }
}
