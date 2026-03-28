using MediatR;
using Polls.Application.Common.Constants;
using Polls.Application.Common.Extensions;
using Polls.Domain.Common;
using Polls.Domain.Polls;

namespace Polls.Application.Polls.Guards;

public sealed class PollGuard(Poll poll)
{
    private Error? _error;

    public static PollGuard For(Poll poll)
    {
        return new PollGuard(poll);
    }

    public PollGuard IsNotFinished()
    {
        if (_error is null && !poll.IsOpen())
            _error = PollErrors.AlreadyFinished(poll.Id);
        return this;
    }

    public PollGuard EditWindowNotExpired()
    {
        if (_error is null)
        {
            var timeSinceCreation = DateTime.UtcNow - poll.CreatedAt;
            if (timeSinceCreation.TotalDays > ValidationConstants.Poll.MaxUpdatePeriodDays)
                _error = PollErrors.UpdatePeriodExpired(ValidationConstants.Poll.MaxUpdatePeriodDays);
        }

        return this;
    }

    public PollGuard DurationIsValid(DateTimeOffset endsAt)
    {
        if (_error is null)
        {
            var totalDuration = endsAt - poll.CreatedAt;
            if (totalDuration.TotalDays > ValidationConstants.Poll.MaxDurationDays)
                _error = PollErrors.MaxDurationExceeded(ValidationConstants.Poll.MaxDurationDays);
        }

        return this;
    }

    public PollGuard SameCity(Guid userCityId)
    {
        if (_error is null && userCityId != poll.CityId)
                _error = PollErrors.NotFromUserCity(poll.Id);

        return this;
    }

    public Result<Unit> Validate()
    {
        if (_error is not null) return _error;

        return Result<Unit>.Success(Unit.Value);
    }
}
