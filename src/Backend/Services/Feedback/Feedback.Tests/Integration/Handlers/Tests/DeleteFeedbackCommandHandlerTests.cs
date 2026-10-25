using ErrorOr;
using Feedback.API.Common.Errors;
using Feedback.API.Features.Feedback.DeleteFeedback;
using Feedback.Tests.Infrastructure;
using Xunit;

namespace Feedback.Tests.Integration.Handlers.Tests;

public sealed class DeleteFeedbackCommandHandlerTests(MongoFixture fixture) : MongoTestsBase(fixture)
{
    private static readonly Guid AuthorId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();

    private DeleteFeedbackCommandHandler CreateSut() => new(Context);

    private static DeleteFeedbackCommand BuildCommand(Guid id, Guid userId, bool bypassRestrictions = false) =>
        new(userId, id, bypassRestrictions);

    [Fact]
    public async Task Handle_FeedbackDoesNotExist_ReturnsNotFoundError()
    {
        var result = await CreateSut().Handle(BuildCommand(Guid.NewGuid(), AuthorId), CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(FeedbackErrors.NotFound, result.FirstError);
    }

    [Fact]
    public async Task Handle_FeedbackOfAnotherUserWithoutBypass_ReturnsCannotDeleteOthersAndKeepsIt()
    {
        var comment = FeedbackItems.Comment(OtherUserId);
        await SeedAsync(comment);

        var result = await CreateSut().Handle(BuildCommand(comment.Id, AuthorId), CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(FeedbackErrors.CannotDeleteOthers, result.FirstError);
        Assert.NotNull(await FindAsync(comment.Id));
    }

    [Fact]
    public async Task Handle_FeedbackOfAnotherUserWithBypass_DeletesIt()
    {
        var comment = FeedbackItems.Comment(OtherUserId);
        await SeedAsync(comment);

        var result = await CreateSut().Handle(
            BuildCommand(comment.Id, AuthorId, bypassRestrictions: true),
            CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(Result.Deleted, result.Value);
        Assert.Null(await FindAsync(comment.Id));
    }

    [Fact]
    public async Task Handle_OwnComment_DeletesOnlyThatComment()
    {
        var deleted = FeedbackItems.Comment(AuthorId);
        var kept = FeedbackItems.Comment(AuthorId, deleted.IdeaId);
        await SeedAsync(deleted, kept);

        var result = await CreateSut().Handle(BuildCommand(deleted.Id, AuthorId), CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Null(await FindAsync(deleted.Id));
        Assert.NotNull(await FindAsync(kept.Id));
    }

    [Fact]
    public async Task Handle_OwnLike_DeletesOnlyThatLike()
    {
        var like = FeedbackItems.Like(AuthorId);
        var otherLike = FeedbackItems.Like(OtherUserId, like.IdeaId);
        await SeedAsync(like, otherLike);

        var result = await CreateSut().Handle(BuildCommand(like.Id, AuthorId), CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Null(await FindAsync(like.Id));
        Assert.NotNull(await FindAsync(otherLike.Id));
    }

    [Fact]
    public async Task Handle_OwnLike_LetsSameUserLikeAgain()
    {
        await Context.CreateIndexesAsync();
        var like = FeedbackItems.Like(AuthorId);
        await SeedAsync(like);

        await CreateSut().Handle(BuildCommand(like.Id, AuthorId), CancellationToken.None);
        var exception = await Record.ExceptionAsync(() => SeedAsync(FeedbackItems.Like(AuthorId, like.IdeaId)));

        Assert.Null(exception);
        Assert.Single(await ReadAllAsync());
    }
}
