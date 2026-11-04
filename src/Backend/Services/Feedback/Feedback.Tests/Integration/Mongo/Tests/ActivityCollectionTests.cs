using Feedback.API.Domain;
using Feedback.Tests.Infrastructure;
using Feedback.Tests.Infrastructure.Constants;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;

namespace Feedback.Tests.Integration.Mongo.Tests;

public sealed class ActivityCollectionTests(MongoFixture fixture) : MongoTestsBase(fixture)
{
    private const string NameField = "name";
    private const string UniqueField = "unique";
    private const string KindField = "Kind";
    private const string OccurredAtField = "OccurredAt";
    private const string IdField = "_id";
    private const string KeyIndex = "Key_1";
    private const string Key = "idea-proposed:1";

    private static readonly string[] ExpectedIndexes =
    [
        KeyIndex,
        "CityId_1_OccurredAt_-1__id_-1",
        "OccurredAt_-1__id_-1",
        "IdeaId_1",
        "PollId_1",
        "UserId_1"
    ];

    private IMongoCollection<BsonDocument> RawActivity =>
        Context.Activity.Database.GetCollection<BsonDocument>(EnvironmentConstants.Mongo.ActivityCollection);

    private async Task<List<BsonDocument>> ListIndexesAsync()
    {
        using var cursor = await Context.Activity.Indexes.ListAsync();

        return await cursor.ToListAsync();
    }

    private static ActivityEntry BuildEntry(string key) => new()
    {
        Key = key,
        Kind = ActivityKind.IdeaProposed,
        CityId = Guid.NewGuid(),
        OccurredAt = Now
    };

    [Fact]
    public async Task CreateIndexesAsync_NewCollection_CreatesActivityIndexes()
    {
        await Context.CreateIndexesAsync();

        var names = (await ListIndexesAsync()).Select(index => index[NameField].AsString).ToList();

        foreach (var expected in ExpectedIndexes)
            Assert.Contains(expected, names);
    }

    [Fact]
    public async Task CreateIndexesAsync_NewCollection_MakesKeyIndexUnique()
    {
        await Context.CreateIndexesAsync();

        var index = (await ListIndexesAsync()).Single(i => i[NameField].AsString == KeyIndex);

        Assert.True(index[UniqueField].AsBoolean);
    }

    [Fact]
    public async Task InsertOneAsync_SameKeyTwice_RejectsDuplicate()
    {
        await Context.CreateIndexesAsync();
        await Context.Activity.InsertOneAsync(BuildEntry(Key));

        var exception = await Assert.ThrowsAsync<MongoWriteException>(
            () => Context.Activity.InsertOneAsync(BuildEntry(Key)));

        Assert.Equal(ServerErrorCategory.DuplicateKey, exception.WriteError.Category);
    }

    [Fact]
    public async Task InsertOneAsync_NewEntry_StoresKindAsStringOccurredAtAsDateAndGeneratesId()
    {
        await Context.Activity.InsertOneAsync(BuildEntry(Key));

        var raw = await RawActivity.Find(FilterDefinition<BsonDocument>.Empty).SingleAsync();

        Assert.Equal(nameof(ActivityKind.IdeaProposed), raw[KindField].AsString);
        Assert.Equal(BsonType.DateTime, raw[OccurredAtField].BsonType);
        Assert.NotEqual(Guid.Empty, raw[IdField].AsGuid);
    }
}
