using Feedback.API.Domain;

namespace Feedback.API.Features.Activity.Live;

public interface IActivityNotifier
{
    Task NotifyAddedAsync(ActivityEntry entry);

    Task NotifyInvalidatedAsync(Guid? cityId);
}
