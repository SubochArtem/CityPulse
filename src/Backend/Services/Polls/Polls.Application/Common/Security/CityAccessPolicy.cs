using Polls.Application.Common.Interfaces;
using Polls.Domain.Common;
using Polls.Domain.Polls;
using MediatR;
namespace Polls.Application.Common.Security;

public sealed class CityAccessPolicy(IUserContextService userContext)
{
    public Result<Unit> Check(Guid targetCityId)
    {
        if (userContext.CityId != targetCityId)
            return PollErrors.NotFromUserCity(targetCityId);

        return Unit.Value; 
    }
}
