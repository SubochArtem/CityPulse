using CityPulse.Contracts.Events.Polls;
using CityPulse.Contracts.Events.Polls.Enums;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;

namespace Polls.Application.Common.Extensions;

public static class IntegrationEventMappings
{
    public static PollLifecycleStatus ToContract(this PollStatus status) =>
        (PollLifecycleStatus)status;

    public static IdeaReviewStatus ToContract(this IdeaApprovalStatus status) =>
        (IdeaReviewStatus)status;

    public static IdeaAccessLevel ToContract(this IdeaAccessStatus status) =>
        (IdeaAccessLevel)status;

    public static PollStatusChangedEvent ToStatusChangedEvent(this Poll poll, DateTimeOffset occurredAt) =>
        new(poll.Id, poll.CityId, poll.Title, poll.Status.ToContract(), occurredAt);

    public static PollDeletedEvent ToDeletedEvent(this Poll poll, DateTimeOffset occurredAt) =>
        new(poll.Id, poll.CityId, occurredAt);

    public static IdeaDeletedEvent ToDeletedEvent(this Idea idea, Poll poll, DateTimeOffset occurredAt) =>
        new(idea.Id, poll.Id, poll.CityId, occurredAt);

    public static IdeaCreatedEvent ToCreatedEvent(this Idea idea, Poll poll, DateTimeOffset occurredAt) =>
        new(idea.Id, poll.Id, poll.CityId, idea.UserId, idea.Title, poll.Title, occurredAt);

    public static IdeaApprovalStatusChangedEvent ToApprovalStatusChangedEvent(
        this Idea idea,
        Poll poll,
        DateTimeOffset occurredAt) =>
        new(
            idea.Id,
            poll.Id,
            poll.CityId,
            idea.UserId,
            idea.Title,
            poll.Title,
            idea.ApprovalStatus.ToContract(),
            occurredAt);

    public static IdeaAccessStatusChangedEvent ToAccessStatusChangedEvent(
        this Idea idea,
        Poll poll,
        DateTimeOffset occurredAt) =>
        new(idea.Id, poll.Id, poll.CityId, idea.AccessStatus.ToContract(), occurredAt);
}
