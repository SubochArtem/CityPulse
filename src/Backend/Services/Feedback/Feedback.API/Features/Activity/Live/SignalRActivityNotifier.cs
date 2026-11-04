using Feedback.API.Domain;
using Microsoft.AspNetCore.SignalR;

namespace Feedback.API.Features.Activity.Live;

public sealed class SignalRActivityNotifier(
    IHubContext<ActivityHub, IActivityClient> hubContext,
    ILogger<SignalRActivityNotifier> logger) : IActivityNotifier
{
    public async Task NotifyAddedAsync(ActivityEntry entry)
    {
        try
        {
            await hubContext.Clients
                .Groups(ActivityGroups.City(entry.CityId), ActivityGroups.All)
                .ActivityAdded(ActivityResponse.FromEntity(entry));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to push activity entry {EntryId} to clients", entry.Id);
        }
    }

    public async Task NotifyInvalidatedAsync(Guid? cityId)
    {
        try
        {
            var clients = cityId.HasValue
                ? hubContext.Clients.Groups(ActivityGroups.City(cityId.Value), ActivityGroups.All)
                : hubContext.Clients.All;

            await clients.ActivityInvalidated();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to push activity invalidation for city {CityId} to clients", cityId);
        }
    }
}
