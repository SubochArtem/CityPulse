using CityPulse.Contracts.Events.Users;
using CityPulse.Contracts.Events.Users.Enums;
using Feedback.API.Features.Activity;
using Feedback.API.Features.Activity.Consumers;
using Feedback.Tests.Infrastructure;
using Xunit;

namespace Feedback.Tests.Integration.Activity.Tests;

public sealed class UserStatusChangedConsumerTests : MongoTestsBase
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly UserStatusChangedConsumer _sut;

    public UserStatusChangedConsumerTests(MongoFixture fixture) : base(fixture)
    {
        _sut = new UserStatusChangedConsumer(new ActivityStore(Context));
    }

    private Task ChangeStatusAsync(UserLifecycleStatus status) =>
        _sut.Consume(ConsumeContexts.For(new UserStatusChangedEvent(_userId, status)));

    [Fact]
    public async Task Inactive_HidesOnlyThatUserEntries()
    {
        var own = ActivityEntries.IdeaProposed(userId: _userId);
        var ownComment = ActivityEntries.IdeaCommented(userId: _userId);
        var other = ActivityEntries.IdeaProposed();
        await SeedActivityAsync(own, ownComment, other);

        await ChangeStatusAsync(UserLifecycleStatus.Inactive);

        var stored = await ReadActivityAsync();
        Assert.True(stored.Single(x => x.Id == own.Id).UserHidden);
        Assert.True(stored.Single(x => x.Id == ownComment.Id).UserHidden);
        Assert.False(stored.Single(x => x.Id == other.Id).UserHidden);
    }

    [Fact]
    public async Task Active_ShowsUserEntriesAgain()
    {
        await SeedActivityAsync(ActivityEntries.IdeaProposed(userId: _userId));
        await ChangeStatusAsync(UserLifecycleStatus.Inactive);

        await ChangeStatusAsync(UserLifecycleStatus.Active);

        Assert.False(Assert.Single(await ReadActivityAsync()).UserHidden);
    }

    [Fact]
    public async Task Undefined_DoesNothing()
    {
        var entry = ActivityEntries.IdeaProposed(userId: _userId);
        entry.UserHidden = true;
        await SeedActivityAsync(entry);

        await ChangeStatusAsync(UserLifecycleStatus.Undefined);

        Assert.True(Assert.Single(await ReadActivityAsync()).UserHidden);
    }
}
