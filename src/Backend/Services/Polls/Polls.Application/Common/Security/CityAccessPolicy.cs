using MediatR;
using Polls.Domain.Common;
using Polls.Domain.Polls;

namespace Polls.Application.Common.Security;

public static class CityAccessPolicy
{
    public static Result<Unit> Check(Guid userCityId, Guid targetCityId)
    {
        if (userCityId != targetCityId)
            return PollErrors.NotFromUserCity(targetCityId);

        return Unit.Value;
    }
}
