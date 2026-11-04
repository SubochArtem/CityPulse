using Feedback.API.Domain;
using Feedback.API.Features.Activity;
using Moq;
using Feedback.Tests.Infrastructure;
using Xunit;

namespace Feedback.Tests.Integration.Activity.Tests;

public sealed class ActivityStoreTests : MongoTestsBase
{
    private readonly ActivityStore _sut;

    public ActivityStoreTests(MongoFixture fixture) : base(fixture)
    {
        _sut = CreateActivityStore();
    }

    [Fact]
    public async Task AddAsync_NewKey_InsertsEntryAndReturnsTrue()
    {
        await Context.CreateIndexesAsync();
        var entry = ActivityEntries.IdeaProposed();

        var added = await _sut.AddAsync(entry);

        Assert.True(added);
        Assert.Equal(entry.Key, Assert.Single(await ReadActivityAsync()).Key);
    }

    [Fact]
    public async Task AddAsync_DuplicateKey_KeepsFirstEntryAndReturnsFalse()
    {
        await Context.CreateIndexesAsync();
        var first = ActivityEntries.IdeaProposed();
        await _sut.AddAsync(first);
        var duplicate = ActivityEntries.IdeaProposed(ideaId: first.IdeaId);

        var added = await _sut.AddAsync(duplicate);

        Assert.False(added);
        Assert.Equal(first.Id, Assert.Single(await ReadActivityAsync()).Id);
    }

    [Fact]
    public async Task AddAsync_RelatedEntriesAreHidden_InheritsHiddenFlags()
    {
        var hiddenPoll = ActivityEntries.PollOpened();
        hiddenPoll.PollHidden = true;
        var hiddenIdea = ActivityEntries.IdeaProposed();
        hiddenIdea.IdeaHidden = true;
        var hiddenUser = ActivityEntries.IdeaProposed();
        hiddenUser.UserHidden = true;
        await SeedActivityAsync(hiddenPoll, hiddenIdea, hiddenUser);
        var comment = ActivityEntries.IdeaCommented(
            pollId: hiddenPoll.PollId,
            ideaId: hiddenIdea.IdeaId,
            userId: hiddenUser.UserId);

        await _sut.AddAsync(comment);

        var stored = (await ReadActivityAsync()).Single(x => x.Key == comment.Key);
        Assert.True(stored.PollHidden);
        Assert.True(stored.IdeaHidden);
        Assert.True(stored.UserHidden);
    }

    [Fact]
    public async Task AddAsync_RelatedEntriesAreVisible_StoresVisibleEntry()
    {
        var sibling = ActivityEntries.IdeaProposed();
        await SeedActivityAsync(sibling);
        var entry = ActivityEntries.PollOpened(pollId: sibling.PollId);

        await _sut.AddAsync(entry);

        var stored = (await ReadActivityAsync()).Single(x => x.Key == entry.Key);
        Assert.False(stored.PollHidden);
        Assert.False(stored.IdeaHidden);
        Assert.False(stored.UserHidden);
    }

    [Fact]
    public async Task SetPollHiddenAsync_TogglesOnlyEntriesOfThatPoll()
    {
        var target = ActivityEntries.PollOpened();
        var targetIdea = ActivityEntries.IdeaProposed(pollId: target.PollId);
        var other = ActivityEntries.PollOpened();
        await SeedActivityAsync(target, targetIdea, other);

        await _sut.SetPollHiddenAsync(target.PollId!.Value, target.CityId, true);

        var stored = await ReadActivityAsync();
        Assert.True(stored.Single(x => x.Id == target.Id).PollHidden);
        Assert.True(stored.Single(x => x.Id == targetIdea.Id).PollHidden);
        Assert.False(stored.Single(x => x.Id == other.Id).PollHidden);

        await _sut.SetPollHiddenAsync(target.PollId!.Value, target.CityId, false);

        Assert.All(await ReadActivityAsync(), x => Assert.False(x.PollHidden));
    }

    [Fact]
    public async Task SetIdeaHiddenAsync_TogglesOnlyEntriesOfThatIdea()
    {
        var target = ActivityEntries.IdeaProposed();
        var other = ActivityEntries.IdeaProposed(pollId: target.PollId);
        await SeedActivityAsync(target, other);

        await _sut.SetIdeaHiddenAsync(target.IdeaId!.Value, target.CityId, true);

        var stored = await ReadActivityAsync();
        Assert.True(stored.Single(x => x.Id == target.Id).IdeaHidden);
        Assert.False(stored.Single(x => x.Id == other.Id).IdeaHidden);
    }

    [Fact]
    public async Task SetUserHiddenAsync_TogglesOnlyEntriesOfThatUser()
    {
        var target = ActivityEntries.IdeaProposed();
        var other = ActivityEntries.IdeaProposed();
        await SeedActivityAsync(target, other);

        await _sut.SetUserHiddenAsync(target.UserId!.Value, true);

        var stored = await ReadActivityAsync();
        Assert.True(stored.Single(x => x.Id == target.Id).UserHidden);
        Assert.False(stored.Single(x => x.Id == other.Id).UserHidden);
    }

    [Fact]
    public async Task DeleteByPollAsync_RemovesPollAndItsIdeaEntries()
    {
        var poll = ActivityEntries.PollOpened();
        var idea = ActivityEntries.IdeaProposed(pollId: poll.PollId);
        var other = ActivityEntries.PollOpened();
        await SeedActivityAsync(poll, idea, other);

        await _sut.DeleteByPollAsync(poll.PollId!.Value, poll.CityId);

        Assert.Equal(other.Id, Assert.Single(await ReadActivityAsync()).Id);
    }

    [Fact]
    public async Task DeleteByIdeaAsync_RemovesOnlyThatIdeaEntries()
    {
        var target = ActivityEntries.IdeaProposed();
        var other = ActivityEntries.IdeaProposed(pollId: target.PollId);
        await SeedActivityAsync(target, other);

        await _sut.DeleteByIdeaAsync(target.IdeaId!.Value, target.CityId);

        Assert.Equal(other.Id, Assert.Single(await ReadActivityAsync()).Id);
    }

    [Fact]
    public async Task FindIdeaProposalAsync_ProposalExists_ReturnsIt()
    {
        var proposal = ActivityEntries.IdeaProposed();
        await SeedActivityAsync(proposal, ActivityEntries.IdeaProposed());

        var found = await _sut.FindIdeaProposalAsync(proposal.IdeaId!.Value);

        Assert.NotNull(found);
        Assert.Equal(proposal.Id, found.Id);
    }

    [Fact]
    public async Task FindIdeaProposalAsync_NoProposal_ReturnsNull()
    {
        var found = await _sut.FindIdeaProposalAsync(Guid.NewGuid());

        Assert.Null(found);
    }

    [Fact]
    public async Task AddAsync_VisibleEntryInserted_NotifiesAdded()
    {
        await Context.CreateIndexesAsync();
        var entry = ActivityEntries.IdeaProposed();

        await _sut.AddAsync(entry);

        ActivityNotifier.Verify(x => x.NotifyAddedAsync(entry), Times.Once);
    }

    [Fact]
    public async Task AddAsync_DuplicateKey_DoesNotNotify()
    {
        await Context.CreateIndexesAsync();
        var first = ActivityEntries.IdeaProposed();
        await SeedActivityAsync(first);

        await _sut.AddAsync(ActivityEntries.IdeaProposed(ideaId: first.IdeaId));

        ActivityNotifier.Verify(x => x.NotifyAddedAsync(It.IsAny<ActivityEntry>()), Times.Never);
    }

    [Fact]
    public async Task AddAsync_HiddenEntryInserted_DoesNotNotify()
    {
        var hiddenIdea = ActivityEntries.IdeaProposed();
        hiddenIdea.IdeaHidden = true;
        await SeedActivityAsync(hiddenIdea);

        await _sut.AddAsync(ActivityEntries.IdeaCommented(ideaId: hiddenIdea.IdeaId));

        ActivityNotifier.Verify(x => x.NotifyAddedAsync(It.IsAny<ActivityEntry>()), Times.Never);
    }

    [Fact]
    public async Task SetPollHiddenAsync_EntriesChange_NotifiesCityOnce()
    {
        var entry = ActivityEntries.PollOpened();
        await SeedActivityAsync(entry, ActivityEntries.IdeaProposed(entry.CityId, entry.PollId));

        await _sut.SetPollHiddenAsync(entry.PollId!.Value, entry.CityId, true);

        ActivityNotifier.Verify(x => x.NotifyInvalidatedAsync(entry.CityId), Times.Once);
    }

    [Fact]
    public async Task SetPollHiddenAsync_NothingChanges_DoesNotNotify()
    {
        var entry = ActivityEntries.PollOpened();
        await SeedActivityAsync(entry);

        await _sut.SetPollHiddenAsync(entry.PollId!.Value, entry.CityId, false);
        await _sut.SetPollHiddenAsync(Guid.NewGuid(), entry.CityId, true);

        ActivityNotifier.Verify(x => x.NotifyInvalidatedAsync(It.IsAny<Guid?>()), Times.Never);
    }

    [Fact]
    public async Task SetIdeaHiddenAsync_EntriesChange_NotifiesCity()
    {
        var entry = ActivityEntries.IdeaProposed();
        await SeedActivityAsync(entry);

        await _sut.SetIdeaHiddenAsync(entry.IdeaId!.Value, entry.CityId, true);

        ActivityNotifier.Verify(x => x.NotifyInvalidatedAsync(entry.CityId), Times.Once);
    }

    [Fact]
    public async Task SetUserHiddenAsync_EntriesChange_NotifiesAllCities()
    {
        var entry = ActivityEntries.IdeaProposed();
        await SeedActivityAsync(entry);

        await _sut.SetUserHiddenAsync(entry.UserId!.Value, true);

        ActivityNotifier.Verify(x => x.NotifyInvalidatedAsync(null), Times.Once);
    }

    [Fact]
    public async Task DeleteByIdeaAsync_EntriesDeleted_NotifiesCity()
    {
        var entry = ActivityEntries.IdeaProposed();
        await SeedActivityAsync(entry);

        await _sut.DeleteByIdeaAsync(entry.IdeaId!.Value, entry.CityId);

        ActivityNotifier.Verify(x => x.NotifyInvalidatedAsync(entry.CityId), Times.Once);
    }

    [Fact]
    public async Task DeleteByPollAsync_NothingDeleted_DoesNotNotify()
    {
        await _sut.DeleteByPollAsync(Guid.NewGuid(), Guid.NewGuid());

        ActivityNotifier.Verify(x => x.NotifyInvalidatedAsync(It.IsAny<Guid?>()), Times.Never);
    }

    [Fact]
    public async Task DeleteByKeyAsync_EntryExists_DeletesItAndNotifiesItsCity()
    {
        var entry = ActivityEntries.IdeaCommented();
        var other = ActivityEntries.IdeaCommented();
        await SeedActivityAsync(entry, other);

        await _sut.DeleteByKeyAsync(entry.Key);

        Assert.Equal(other.Id, Assert.Single(await ReadActivityAsync()).Id);
        ActivityNotifier.Verify(x => x.NotifyInvalidatedAsync(entry.CityId), Times.Once);
    }

    [Fact]
    public async Task DeleteByKeyAsync_EntryMissing_DoesNotNotify()
    {
        await _sut.DeleteByKeyAsync(ActivityKeys.IdeaCommented(Guid.NewGuid()));

        ActivityNotifier.Verify(x => x.NotifyInvalidatedAsync(It.IsAny<Guid?>()), Times.Never);
    }
}
