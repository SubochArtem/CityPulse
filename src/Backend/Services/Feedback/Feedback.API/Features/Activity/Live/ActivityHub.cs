using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Feedback.API.Features.Activity.Live;

[AllowAnonymous]
public sealed class ActivityHub : Hub<IActivityClient>
{
    public const string Route = "/hubs/activity";
    public const string EmptyCityMessage = "City id must not be empty.";

    private const string GroupKey = "activity-group";

    public async Task Watch(Guid? cityId)
    {
        if (cityId == Guid.Empty)
            throw new HubException(EmptyCityMessage);

        var group = ActivityGroups.For(cityId);

        if (Context.Items.TryGetValue(GroupKey, out var current) && current is string currentGroup)
        {
            if (currentGroup == group)
                return;

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, currentGroup);
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, group);
        Context.Items[GroupKey] = group;
    }
}
