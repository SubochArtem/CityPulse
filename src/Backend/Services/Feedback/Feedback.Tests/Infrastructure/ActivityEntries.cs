using Feedback.API.Domain;
using Feedback.API.Features.Activity;

namespace Feedback.Tests.Infrastructure;

internal static class ActivityEntries
{
    public static readonly DateTimeOffset DefaultOccurredAt = new(2026, 11, 1, 12, 0, 0, TimeSpan.Zero);

    public static ActivityEntry IdeaProposed(
        Guid? cityId = null,
        Guid? pollId = null,
        Guid? ideaId = null,
        Guid? userId = null,
        DateTimeOffset? occurredAt = null)
    {
        var id = ideaId ?? Guid.NewGuid();

        return new ActivityEntry
        {
            Id = Guid.NewGuid(),
            Key = ActivityKeys.IdeaProposed(id),
            Kind = ActivityKind.IdeaProposed,
            CityId = cityId ?? Guid.NewGuid(),
            PollId = pollId ?? Guid.NewGuid(),
            PollTitle = "Old market",
            IdeaId = id,
            IdeaTitle = "Night buses",
            UserId = userId ?? Guid.NewGuid(),
            OccurredAt = occurredAt ?? DefaultOccurredAt
        };
    }

    public static ActivityEntry PollOpened(
        Guid? cityId = null,
        Guid? pollId = null,
        DateTimeOffset? occurredAt = null)
    {
        var id = pollId ?? Guid.NewGuid();

        return new ActivityEntry
        {
            Id = Guid.NewGuid(),
            Key = ActivityKeys.PollOpened(id),
            Kind = ActivityKind.PollOpened,
            CityId = cityId ?? Guid.NewGuid(),
            PollId = id,
            PollTitle = "Old market",
            OccurredAt = occurredAt ?? DefaultOccurredAt
        };
    }

    public static ActivityEntry IdeaCommented(
        Guid? cityId = null,
        Guid? pollId = null,
        Guid? ideaId = null,
        Guid? userId = null,
        DateTimeOffset? occurredAt = null)
    {
        var feedbackId = Guid.NewGuid();

        return new ActivityEntry
        {
            Id = Guid.NewGuid(),
            Key = ActivityKeys.IdeaCommented(feedbackId),
            Kind = ActivityKind.IdeaCommented,
            CityId = cityId ?? Guid.NewGuid(),
            PollId = pollId ?? Guid.NewGuid(),
            IdeaId = ideaId ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            FeedbackId = feedbackId,
            OccurredAt = occurredAt ?? DefaultOccurredAt
        };
    }
}
