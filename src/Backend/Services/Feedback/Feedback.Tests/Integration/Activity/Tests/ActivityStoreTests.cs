using Feedback.API.Features.Activity;
using Feedback.Tests.Infrastructure;
using Xunit;

namespace Feedback.Tests.Integration.Activity.Tests;

public sealed class ActivityStoreTests : MongoTestsBase
{
    private readonly ActivityStore _sut;

    public ActivityStoreTests(MongoFixture fixture) : base(fixture)
    {
        _sut = new ActivityStore(Context);
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

        await _sut.SetPollHiddenAsync(target.PollId!.Value, true);

        var stored = await ReadActivityAsync();
        Assert.True(stored.Single(x => x.Id == target.Id).PollHidden);
        Assert.True(stored.Single(x => x.Id == targetIdea.Id).PollHidden);
        Assert.False(stored.Single(x => x.Id == other.Id).PollHidden);

        await _sut.SetPollHiddenAsync(target.PollId!.Value, false);

        Assert.All(await ReadActivityAsync(), x => Assert.False(x.PollHidden));
    }

    [Fact]
    public async Task SetIdeaHiddenAsync_TogglesOnlyEntriesOfThatIdea()
    {
        var target = ActivityEntries.IdeaProposed();
        var other = ActivityEntries.IdeaProposed(pollId: target.PollId);
        await SeedActivityAsync(target, other);

        await _sut.SetIdeaHiddenAsync(target.IdeaId!.Value, true);

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

        await _sut.DeleteByPollAsync(poll.PollId!.Value);

        Assert.Equal(other.Id, Assert.Single(await ReadActivityAsync()).Id);
    }

    [Fact]
    public async Task DeleteByIdeaAsync_RemovesOnlyThatIdeaEntries()
    {
        var target = ActivityEntries.IdeaProposed();
        var other = ActivityEntries.IdeaProposed(pollId: target.PollId);
        await SeedActivityAsync(target, other);

        await _sut.DeleteByIdeaAsync(target.IdeaId!.Value);

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
}
