using Feedback.Tests.Infrastructure;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;

namespace Feedback.Tests.Integration.Mongo.Tests;

public sealed class MongoDbContextTests(MongoFixture fixture) : MongoTestsBase(fixture)
{
    private const string IdField = "_id";
    private const string CreatedAtField = "CreatedAt";
    private const string UpdatedAtField = "UpdatedAt";
    private const string TextField = "Text";
    private const string NameField = "name";
    private const string UniqueField = "unique";
    private const string PartialFilterField = "partialFilterExpression";
    private const string UniqueLikeIndex = "UserId_1_IdeaId_1";

    private static readonly string[] ExpectedIndexes =
    [
        "IdeaId_1_CreatedAt_-1",
        "UserId_1_CreatedAt_-1",
        "CreatedAt_-1",
        UniqueLikeIndex
    ];

    private async Task<List<BsonDocument>> ListIndexesAsync()
    {
        using var cursor = await Context.FeedbackItems.Indexes.ListAsync();

        return await cursor.ToListAsync();
    }

    private static bool IsDuplicateKey(MongoWriteException exception) =>
        exception.WriteError.Category == ServerErrorCategory.DuplicateKey;

    [Fact]
    public async Task CreateIndexesAsync_NewCollection_CreatesExpectedIndexes()
    {
        await Context.CreateIndexesAsync();

        var names = (await ListIndexesAsync()).Select(index => index[NameField].AsString).ToList();

        foreach (var expected in ExpectedIndexes)
            Assert.Contains(expected, names);
    }

    [Fact]
    public async Task CreateIndexesAsync_NewCollection_MakesUserIdeaIndexUniqueAndPartialForLikes()
    {
        await Context.CreateIndexesAsync();

        var index = (await ListIndexesAsync()).Single(i => i[NameField].AsString == UniqueLikeIndex);

        Assert.True(index[UniqueField].AsBoolean);
        Assert.True(index[PartialFilterField].AsBsonDocument[TextField].IsBsonNull);
    }

    [Fact]
    public async Task CreateIndexesAsync_CalledTwice_DoesNotThrow()
    {
        await Context.CreateIndexesAsync();

        var exception = await Record.ExceptionAsync(() => Context.CreateIndexesAsync());

        Assert.Null(exception);
    }

    [Fact]
    public async Task InsertOne_TwoLikesFromSameUserOnSameIdea_ViolatesUniqueIndex()
    {
        await Context.CreateIndexesAsync();
        var userId = Guid.NewGuid();
        var ideaId = Guid.NewGuid();
        await Context.FeedbackItems.InsertOneAsync(FeedbackItems.Like(userId, ideaId));

        var exception = await Assert.ThrowsAsync<MongoWriteException>(
            () => Context.FeedbackItems.InsertOneAsync(FeedbackItems.Like(userId, ideaId)));

        Assert.True(IsDuplicateKey(exception));
        Assert.Single(await ReadAllAsync());
    }

    [Fact]
    public async Task InsertMany_CommentsAndLikeFromSameUserOnSameIdea_AreAllowed()
    {
        await Context.CreateIndexesAsync();
        var userId = Guid.NewGuid();
        var ideaId = Guid.NewGuid();

        await SeedAsync(
            FeedbackItems.Comment(userId, ideaId, "same text"),
            FeedbackItems.Comment(userId, ideaId, "same text"),
            FeedbackItems.Like(userId, ideaId));

        Assert.Equal(3, (await ReadAllAsync()).Count);
    }

    [Fact]
    public async Task InsertMany_LikesFromDifferentUsersOrOnDifferentIdeas_AreAllowed()
    {
        await Context.CreateIndexesAsync();
        var userId = Guid.NewGuid();
        var ideaId = Guid.NewGuid();

        await SeedAsync(
            FeedbackItems.Like(userId, ideaId),
            FeedbackItems.Like(Guid.NewGuid(), ideaId),
            FeedbackItems.Like(userId, Guid.NewGuid()));

        Assert.Equal(3, (await ReadAllAsync()).Count);
    }

    [Fact]
    public async Task InsertOne_Comment_StoresGuidAsStandardBinaryAndDatesAsDateTime()
    {
        var comment = FeedbackItems.Comment(updatedAt: FeedbackItems.DefaultCreatedAt.AddHours(1));
        await SeedAsync(comment);

        var document = await RawItems.Find(FilterDefinition<BsonDocument>.Empty).SingleAsync();

        Assert.Equal(BsonBinarySubType.UuidStandard, document[IdField].AsBsonBinaryData.SubType);
        Assert.Equal(BsonType.DateTime, document[CreatedAtField].BsonType);
        Assert.Equal(BsonType.DateTime, document[UpdatedAtField].BsonType);
        Assert.Equal(comment.Text, document[TextField].AsString);
    }

    [Fact]
    public async Task InsertOne_Like_StoresNoTextFieldAndNullUpdatedAt()
    {
        await SeedAsync(FeedbackItems.Like());

        var document = await RawItems.Find(FilterDefinition<BsonDocument>.Empty).SingleAsync();

        Assert.False(document.Contains(TextField));
        Assert.True(document[UpdatedAtField].IsBsonNull);
    }

    [Fact]
    public async Task InsertOne_Comment_RoundTripsAllFields()
    {
        var comment = FeedbackItems.Comment(updatedAt: FeedbackItems.DefaultCreatedAt.AddHours(1));
        await SeedAsync(comment);

        var stored = await FindAsync(comment.Id);

        Assert.NotNull(stored);
        Assert.Equal(comment.UserId, stored.UserId);
        Assert.Equal(comment.IdeaId, stored.IdeaId);
        Assert.Equal(comment.Text, stored.Text);
        Assert.Equal(comment.CreatedAt, stored.CreatedAt);
        Assert.Equal(comment.UpdatedAt, stored.UpdatedAt);
    }
}
