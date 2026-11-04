namespace Feedback.API.Features.Activity.Live;

public interface IActivityClient
{
    Task ActivityAdded(ActivityResponse entry);

    Task ActivityInvalidated();
}
