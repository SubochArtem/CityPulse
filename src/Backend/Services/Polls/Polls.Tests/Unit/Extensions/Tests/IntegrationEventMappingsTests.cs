using CityPulse.Contracts.Events.Polls.Enums;
using Polls.Application.Common.Extensions;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Polls.Enums;
using Polls.Tests.TestData;
using Xunit;

namespace Polls.Tests.Unit.Extensions.Tests;

public sealed class IntegrationEventMappingsTests
{
    private static readonly DateTimeOffset OccurredAt = new(2026, 11, 5, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void PollStatus_MatchesContractEnum()
    {
        AssertEnumsMatch<PollStatus, PollLifecycleStatus>();
    }

    [Fact]
    public void IdeaApprovalStatus_MatchesContractEnum()
    {
        AssertEnumsMatch<IdeaApprovalStatus, IdeaReviewStatus>();
    }

    [Fact]
    public void IdeaAccessStatus_MatchesContractEnum()
    {
        AssertEnumsMatch<IdeaAccessStatus, IdeaAccessLevel>();
    }

    [Fact]
    public void ToStatusChangedEvent_CopiesPollSnapshot()
    {
        var poll = PollFakers.Polls().Generate();

        var message = poll.ToStatusChangedEvent(OccurredAt);

        Assert.Equal(poll.Id, message.PollId);
        Assert.Equal(poll.CityId, message.CityId);
        Assert.Equal(poll.Title, message.Title);
        Assert.Equal(poll.Status.ToContract(), message.Status);
        Assert.Equal(OccurredAt, message.OccurredAt);
    }

    [Fact]
    public void ToCreatedEvent_CopiesIdeaAndPollSnapshot()
    {
        var idea = IdeaFakers.IdeaWithPoll();

        var message = idea.ToCreatedEvent(idea.Poll, OccurredAt);

        Assert.Equal(idea.Id, message.IdeaId);
        Assert.Equal(idea.Poll.Id, message.PollId);
        Assert.Equal(idea.Poll.CityId, message.CityId);
        Assert.Equal(idea.UserId, message.UserId);
        Assert.Equal(idea.Title, message.Title);
        Assert.Equal(idea.Poll.Title, message.PollTitle);
        Assert.Equal(OccurredAt, message.OccurredAt);
    }

    [Fact]
    public void ToApprovalStatusChangedEvent_CopiesApprovalStatus()
    {
        var idea = IdeaFakers.IdeaWithPoll();
        idea.ApprovalStatus = IdeaApprovalStatus.Approved;

        var message = idea.ToApprovalStatusChangedEvent(idea.Poll, OccurredAt);

        Assert.Equal(IdeaReviewStatus.Approved, message.ApprovalStatus);
        Assert.Equal(idea.Poll.CityId, message.CityId);
        Assert.Equal(idea.Poll.Title, message.PollTitle);
    }

    private static void AssertEnumsMatch<TDomain, TContract>()
        where TDomain : struct, Enum
        where TContract : struct, Enum
    {
        var domain = Enum.GetValues<TDomain>().ToDictionary(v => v.ToString(), v => Convert.ToInt32(v));
        var contract = Enum.GetValues<TContract>().ToDictionary(v => v.ToString(), v => Convert.ToInt32(v));

        Assert.Equal(domain, contract);
    }
}
