using Feedback.API.Common.Errors;
using Feedback.API.Features.Feedback.EditComment;
using Feedback.Tests.Infrastructure;
using Feedback.Tests.Integration.Handlers.TestData;
using Xunit;

namespace Feedback.Tests.Integration.Handlers.Tests;

public sealed class EditCommentCommandHandlerTests(MongoFixture fixture) : MongoTestsBase(fixture)
{
    private const string OriginalText = "Original comment";
    private const string NewText = "Edited comment";
    private const string PaddedNewText = "  Edited comment  ";

    private static readonly Guid AuthorId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();

    private EditCommentCommandHandler CreateSut() => new(Context, DateTimeProvider.Object);

    private static EditCommentCommand BuildCommand(
        Guid id,
        Guid userId,
        string text = NewText,
        bool bypassRestrictions = false) =>
        new(userId, id, text, bypassRestrictions);

    [Fact]
    public async Task Handle_FeedbackDoesNotExist_ReturnsNotFoundError()
    {
        var result = await CreateSut().Handle(BuildCommand(Guid.NewGuid(), AuthorId), CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(FeedbackErrors.NotFound, result.FirstError);
    }

    [Theory]
    [MemberData(nameof(AddFeedbackTestData.BypassRestrictionsValues), MemberType = typeof(AddFeedbackTestData))]
    public async Task Handle_FeedbackIsLike_ReturnsNotACommentErrorAndKeepsItUnchanged(bool bypassRestrictions)
    {
        var like = FeedbackItems.Like(AuthorId);
        await SeedAsync(like);

        var result = await CreateSut().Handle(
            BuildCommand(like.Id, AuthorId, bypassRestrictions: bypassRestrictions),
            CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(FeedbackErrors.NotAComment, result.FirstError);
        var stored = await FindAsync(like.Id);
        Assert.NotNull(stored);
        Assert.Null(stored.Text);
        Assert.Null(stored.UpdatedAt);
    }

    [Fact]
    public async Task Handle_LikeOfAnotherUserWithoutBypass_ReturnsNotACommentBeforeOwnershipCheck()
    {
        var like = FeedbackItems.Like(OtherUserId);
        await SeedAsync(like);

        var result = await CreateSut().Handle(BuildCommand(like.Id, AuthorId), CancellationToken.None);

        Assert.Equal(FeedbackErrors.NotAComment, result.FirstError);
    }

    [Fact]
    public async Task Handle_CommentOfAnotherUserWithoutBypass_ReturnsCannotEditOthersAndKeepsItUnchanged()
    {
        var comment = FeedbackItems.Comment(OtherUserId, text: OriginalText);
        await SeedAsync(comment);

        var result = await CreateSut().Handle(BuildCommand(comment.Id, AuthorId), CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(FeedbackErrors.CannotEditOthers, result.FirstError);
        var stored = await FindAsync(comment.Id);
        Assert.NotNull(stored);
        Assert.Equal(OriginalText, stored.Text);
        Assert.Null(stored.UpdatedAt);
    }

    [Fact]
    public async Task Handle_CommentOfAnotherUserWithBypass_EditsItAndKeepsOriginalAuthor()
    {
        var comment = FeedbackItems.Comment(OtherUserId, text: OriginalText);
        await SeedAsync(comment);

        var result = await CreateSut().Handle(
            BuildCommand(comment.Id, AuthorId, bypassRestrictions: true),
            CancellationToken.None);

        Assert.False(result.IsError);
        var stored = await FindAsync(comment.Id);
        Assert.NotNull(stored);
        Assert.Equal(NewText, stored.Text);
        Assert.Equal(OtherUserId, stored.UserId);
    }

    [Fact]
    public async Task Handle_OwnComment_StoresTrimmedTextAndUpdateTime()
    {
        var comment = FeedbackItems.Comment(AuthorId, text: OriginalText);
        await SeedAsync(comment);

        await CreateSut().Handle(BuildCommand(comment.Id, AuthorId, PaddedNewText), CancellationToken.None);

        var stored = await FindAsync(comment.Id);
        Assert.NotNull(stored);
        Assert.Equal(NewText, stored.Text);
        Assert.Equal(Now, stored.UpdatedAt);
    }

    [Fact]
    public async Task Handle_OwnComment_KeepsOtherFieldsUnchanged()
    {
        var comment = FeedbackItems.Comment(AuthorId, text: OriginalText);
        await SeedAsync(comment);

        await CreateSut().Handle(BuildCommand(comment.Id, AuthorId), CancellationToken.None);

        var stored = await FindAsync(comment.Id);
        Assert.NotNull(stored);
        Assert.Equal(comment.Id, stored.Id);
        Assert.Equal(AuthorId, stored.UserId);
        Assert.Equal(comment.IdeaId, stored.IdeaId);
        Assert.Equal(comment.CreatedAt, stored.CreatedAt);
    }

    [Fact]
    public async Task Handle_OwnComment_ReturnsResponseMatchingStoredItem()
    {
        var comment = FeedbackItems.Comment(AuthorId, text: OriginalText);
        await SeedAsync(comment);

        var result = await CreateSut().Handle(BuildCommand(comment.Id, AuthorId), CancellationToken.None);

        var stored = await FindAsync(comment.Id);
        Assert.False(result.IsError);
        Assert.NotNull(stored);
        Assert.Equal(stored.Id, result.Value.Id);
        Assert.Equal(stored.Text, result.Value.Text);
        Assert.Equal(stored.CreatedAt, result.Value.CreatedAt);
        Assert.Equal(stored.UpdatedAt, result.Value.UpdatedAt);
    }

    [Fact]
    public async Task Handle_OwnComment_DoesNotTouchOtherItems()
    {
        var edited = FeedbackItems.Comment(AuthorId, text: OriginalText);
        var neighbor = FeedbackItems.Comment(AuthorId, edited.IdeaId, OriginalText);
        await SeedAsync(edited, neighbor);

        await CreateSut().Handle(BuildCommand(edited.Id, AuthorId), CancellationToken.None);

        var storedNeighbor = await FindAsync(neighbor.Id);
        Assert.NotNull(storedNeighbor);
        Assert.Equal(OriginalText, storedNeighbor.Text);
        Assert.Null(storedNeighbor.UpdatedAt);
    }
}
