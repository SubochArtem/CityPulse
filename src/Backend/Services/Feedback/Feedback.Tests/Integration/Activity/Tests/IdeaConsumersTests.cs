using CityPulse.Contracts.Events.Polls;
using CityPulse.Contracts.Events.Polls.Enums;
using Feedback.API.Domain;
using Feedback.API.Features.Activity;
using Feedback.API.Features.Activity.Consumers;
using Feedback.Tests.Infrastructure;
using Xunit;

namespace Feedback.Tests.Integration.Activity.Tests;

public sealed class IdeaConsumersTests : MongoTestsBase
{
    private const string IdeaTitle = "Night buses";
    private const string PollTitle = "Old market";

    private readonly Guid _ideaId = Guid.NewGuid();
    private readonly Guid _pollId = Guid.NewGuid();
    private readonly Guid _cityId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly IdeaCreatedConsumer _createdConsumer;
    private readonly IdeaApprovalStatusChangedConsumer _approvalConsumer;
    private readonly IdeaAccessStatusChangedConsumer _accessConsumer;
    private readonly IdeaDeletedConsumer _deletedConsumer;

    public IdeaConsumersTests(MongoFixture fixture) : base(fixture)
    {
        var store = CreateActivityStore();
        _createdConsumer = new IdeaCreatedConsumer(store);
        _approvalConsumer = new IdeaApprovalStatusChangedConsumer(store);
        _accessConsumer = new IdeaAccessStatusChangedConsumer(store);
        _deletedConsumer = new IdeaDeletedConsumer(store);
    }

    private Task CreateAsync() =>
        _createdConsumer.Consume(ConsumeContexts.For(
            new IdeaCreatedEvent(_ideaId, _pollId, _cityId, _userId, IdeaTitle, PollTitle, Now)));

    private Task ReviewAsync(IdeaReviewStatus status) =>
        _approvalConsumer.Consume(ConsumeContexts.For(
            new IdeaApprovalStatusChangedEvent(_ideaId, _pollId, _cityId, _userId, IdeaTitle, PollTitle, status, Now)));

    private Task ChangeAccessAsync(IdeaAccessLevel status) =>
        _accessConsumer.Consume(ConsumeContexts.For(
            new IdeaAccessStatusChangedEvent(_ideaId, _pollId, _cityId, status, Now)));

    [Fact]
    public async Task Created_AddsIdeaProposedEntryWithSnapshot()
    {
        await Context.CreateIndexesAsync();

        await CreateAsync();

        var entry = Assert.Single(await ReadActivityAsync());
        Assert.Equal(ActivityKind.IdeaProposed, entry.Kind);
        Assert.Equal(ActivityKeys.IdeaProposed(_ideaId), entry.Key);
        Assert.Equal(_cityId, entry.CityId);
        Assert.Equal(_pollId, entry.PollId);
        Assert.Equal(PollTitle, entry.PollTitle);
        Assert.Equal(_ideaId, entry.IdeaId);
        Assert.Equal(IdeaTitle, entry.IdeaTitle);
        Assert.Equal(_userId, entry.UserId);
        Assert.Equal(Now, entry.OccurredAt);
    }

    [Fact]
    public async Task Created_DeliveredTwice_KeepsSingleEntry()
    {
        await Context.CreateIndexesAsync();

        await CreateAsync();
        await CreateAsync();

        Assert.Single(await ReadActivityAsync());
    }

    [Theory]
    [InlineData(IdeaReviewStatus.Approved)]
    [InlineData(IdeaReviewStatus.Rejected)]
    public async Task ApprovalChanged_FinalDecision_AddsIdeaReviewedEntry(IdeaReviewStatus status)
    {
        await Context.CreateIndexesAsync();

        await ReviewAsync(status);

        var entry = Assert.Single(await ReadActivityAsync());
        Assert.Equal(ActivityKind.IdeaReviewed, entry.Kind);
        Assert.Equal(status.ToString(), entry.Status);
        Assert.Equal(IdeaTitle, entry.IdeaTitle);
        Assert.Equal(_userId, entry.UserId);
    }

    [Theory]
    [InlineData(IdeaReviewStatus.Undefined)]
    [InlineData(IdeaReviewStatus.Pending)]
    [InlineData(IdeaReviewStatus.Deferred)]
    public async Task ApprovalChanged_NotFinalDecision_DoesNothing(IdeaReviewStatus status)
    {
        await ReviewAsync(status);

        Assert.Empty(await ReadActivityAsync());
    }

    [Fact]
    public async Task ApprovalChanged_IdeaIsHidden_StoresHiddenEntry()
    {
        await Context.CreateIndexesAsync();
        await CreateAsync();
        await ChangeAccessAsync(IdeaAccessLevel.Inactive);

        await ReviewAsync(IdeaReviewStatus.Approved);

        Assert.All(await ReadActivityAsync(), x => Assert.True(x.IdeaHidden));
    }

    [Theory]
    [InlineData(IdeaAccessLevel.Inactive)]
    [InlineData(IdeaAccessLevel.RestrictedByContext)]
    [InlineData(IdeaAccessLevel.RestrictedByAuthor)]
    public async Task AccessChanged_NotActive_HidesIdeaEntries(IdeaAccessLevel status)
    {
        await CreateAsync();

        await ChangeAccessAsync(status);

        Assert.True(Assert.Single(await ReadActivityAsync()).IdeaHidden);
    }

    [Fact]
    public async Task AccessChanged_BackToActive_ShowsIdeaEntries()
    {
        await CreateAsync();
        await ChangeAccessAsync(IdeaAccessLevel.Inactive);

        await ChangeAccessAsync(IdeaAccessLevel.Active);

        Assert.False(Assert.Single(await ReadActivityAsync()).IdeaHidden);
    }

    [Fact]
    public async Task AccessChanged_Undefined_DoesNothing()
    {
        await CreateAsync();

        await ChangeAccessAsync(IdeaAccessLevel.Undefined);

        Assert.False(Assert.Single(await ReadActivityAsync()).IdeaHidden);
    }

    [Fact]
    public async Task Deleted_RemovesIdeaEntriesOnly()
    {
        var other = ActivityEntries.IdeaProposed(_cityId, _pollId);
        await SeedActivityAsync(
            ActivityEntries.IdeaProposed(_cityId, _pollId, _ideaId),
            ActivityEntries.IdeaCommented(_cityId, _pollId, _ideaId),
            other);

        await _deletedConsumer.Consume(ConsumeContexts.For(new IdeaDeletedEvent(_ideaId, _pollId, _cityId, Now)));

        Assert.Equal(other.Id, Assert.Single(await ReadActivityAsync()).Id);
    }
}
