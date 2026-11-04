using CityPulse.Contracts.Events.Polls;
using CityPulse.Contracts.Events.Polls.Enums;
using Feedback.API.Domain;
using Feedback.API.Features.Activity;
using Feedback.API.Features.Activity.Consumers;
using Feedback.Tests.Infrastructure;
using Xunit;

namespace Feedback.Tests.Integration.Activity.Tests;

public sealed class PollConsumersTests : MongoTestsBase
{
    private const string PollTitle = "Old market";

    private readonly Guid _pollId = Guid.NewGuid();
    private readonly Guid _cityId = Guid.NewGuid();
    private readonly PollStatusChangedConsumer _statusConsumer;
    private readonly PollDeletedConsumer _deletedConsumer;

    public PollConsumersTests(MongoFixture fixture) : base(fixture)
    {
        var store = CreateActivityStore();
        _statusConsumer = new PollStatusChangedConsumer(store);
        _deletedConsumer = new PollDeletedConsumer(store);
    }

    private Task ChangeStatusAsync(PollLifecycleStatus status) =>
        _statusConsumer.Consume(ConsumeContexts.For(
            new PollStatusChangedEvent(_pollId, _cityId, PollTitle, status, Now)));

    [Fact]
    public async Task StatusChanged_Active_AddsPollOpenedEntry()
    {
        await Context.CreateIndexesAsync();

        await ChangeStatusAsync(PollLifecycleStatus.Active);

        var entry = Assert.Single(await ReadActivityAsync());
        Assert.Equal(ActivityKind.PollOpened, entry.Kind);
        Assert.Equal(ActivityKeys.PollOpened(_pollId), entry.Key);
        Assert.Equal(_cityId, entry.CityId);
        Assert.Equal(_pollId, entry.PollId);
        Assert.Equal(PollTitle, entry.PollTitle);
        Assert.Equal(Now, entry.OccurredAt);
        Assert.Null(entry.Status);
    }

    [Fact]
    public async Task StatusChanged_ActiveDeliveredTwice_KeepsSingleEntry()
    {
        await Context.CreateIndexesAsync();

        await ChangeStatusAsync(PollLifecycleStatus.Active);
        await ChangeStatusAsync(PollLifecycleStatus.Active);

        Assert.Single(await ReadActivityAsync());
    }

    [Fact]
    public async Task StatusChanged_SuspendedThenActive_HidesThenShowsPollEntries()
    {
        await Context.CreateIndexesAsync();
        var idea = ActivityEntries.IdeaProposed(_cityId, _pollId);
        await SeedActivityAsync(idea);
        await ChangeStatusAsync(PollLifecycleStatus.Active);

        await ChangeStatusAsync(PollLifecycleStatus.Suspended);

        Assert.All(await ReadActivityAsync(), x => Assert.True(x.PollHidden));

        await ChangeStatusAsync(PollLifecycleStatus.Active);

        var entries = await ReadActivityAsync();
        Assert.Equal(2, entries.Count);
        Assert.All(entries, x => Assert.False(x.PollHidden));
    }

    [Theory]
    [InlineData(PollLifecycleStatus.Finished)]
    [InlineData(PollLifecycleStatus.Cancelled)]
    [InlineData(PollLifecycleStatus.Inactive)]
    public async Task StatusChanged_Closed_AddsPollClosedEntryWithStatus(PollLifecycleStatus status)
    {
        await Context.CreateIndexesAsync();

        await ChangeStatusAsync(status);

        var entry = Assert.Single(await ReadActivityAsync());
        Assert.Equal(ActivityKind.PollClosed, entry.Kind);
        Assert.Equal(status.ToString(), entry.Status);
        Assert.Equal(ActivityKeys.PollClosed(_pollId, status.ToString()), entry.Key);
    }

    [Fact]
    public async Task StatusChanged_Undefined_DoesNothing()
    {
        await ChangeStatusAsync(PollLifecycleStatus.Undefined);

        Assert.Empty(await ReadActivityAsync());
    }

    [Fact]
    public async Task PollDeleted_RemovesAllPollEntries()
    {
        var other = ActivityEntries.PollOpened(_cityId);
        await SeedActivityAsync(
            ActivityEntries.PollOpened(_cityId, _pollId),
            ActivityEntries.IdeaProposed(_cityId, _pollId),
            other);

        await _deletedConsumer.Consume(ConsumeContexts.For(new PollDeletedEvent(_pollId, _cityId, Now)));

        Assert.Equal(other.Id, Assert.Single(await ReadActivityAsync()).Id);
    }
}
