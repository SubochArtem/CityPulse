using CityPulse.Contracts.Events.Polls;
using CityPulse.Contracts.Events.Polls.Enums;
using Feedback.API.Domain;
using MassTransit;

namespace Feedback.API.Features.Activity.Consumers;

public sealed class PollStatusChangedConsumer(ActivityStore store) : IConsumer<PollStatusChangedEvent>
{
    public async Task Consume(ConsumeContext<PollStatusChangedEvent> context)
    {
        var message = context.Message;
        var cancellationToken = context.CancellationToken;

        switch (message.Status)
        {
            case PollLifecycleStatus.Active:
                await store.SetPollHiddenAsync(message.PollId, false, cancellationToken);
                await store.AddAsync(
                    BuildEntry(message, ActivityKind.PollOpened, ActivityKeys.PollOpened(message.PollId)),
                    cancellationToken);
                break;
            case PollLifecycleStatus.Suspended:
                await store.SetPollHiddenAsync(message.PollId, true, cancellationToken);
                break;
            case PollLifecycleStatus.Finished:
            case PollLifecycleStatus.Cancelled:
            case PollLifecycleStatus.Inactive:
                await AddClosedAsync(message, cancellationToken);
                break;
            default:
                break;
        }
    }

    private Task AddClosedAsync(PollStatusChangedEvent message, CancellationToken cancellationToken)
    {
        var status = message.Status.ToString();

        return store.AddAsync(
            BuildEntry(message, ActivityKind.PollClosed, ActivityKeys.PollClosed(message.PollId, status), status),
            cancellationToken);
    }

    private static ActivityEntry BuildEntry(
        PollStatusChangedEvent message,
        ActivityKind kind,
        string key,
        string? status = null) => new()
    {
        Key = key,
        Kind = kind,
        CityId = message.CityId,
        PollId = message.PollId,
        PollTitle = message.Title,
        Status = status,
        OccurredAt = message.OccurredAt
    };
}
