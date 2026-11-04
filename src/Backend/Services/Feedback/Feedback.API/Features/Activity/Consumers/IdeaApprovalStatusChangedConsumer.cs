using CityPulse.Contracts.Events.Polls;
using CityPulse.Contracts.Events.Polls.Enums;
using Feedback.API.Domain;
using MassTransit;

namespace Feedback.API.Features.Activity.Consumers;

public sealed class IdeaApprovalStatusChangedConsumer(ActivityStore store)
    : IConsumer<IdeaApprovalStatusChangedEvent>
{
    public Task Consume(ConsumeContext<IdeaApprovalStatusChangedEvent> context)
    {
        var message = context.Message;
        if (message.ApprovalStatus is not (IdeaReviewStatus.Approved or IdeaReviewStatus.Rejected))
            return Task.CompletedTask;

        var status = message.ApprovalStatus.ToString();
        var entry = new ActivityEntry
        {
            Key = ActivityKeys.IdeaReviewed(message.IdeaId, status),
            Kind = ActivityKind.IdeaReviewed,
            CityId = message.CityId,
            PollId = message.PollId,
            PollTitle = message.PollTitle,
            IdeaId = message.IdeaId,
            IdeaTitle = message.Title,
            UserId = message.UserId,
            Status = status,
            OccurredAt = message.OccurredAt
        };

        return store.AddAsync(entry, context.CancellationToken);
    }
}
