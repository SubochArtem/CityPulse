using Feedback.API.Domain;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Feedback.API.Infrastructure.Mongo;

public class MongoDbContext
{
    static MongoDbContext()
    {
        BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

        RegisterFeedbackItemMap();
        RegisterActivityEntryMap();
    }

    private static void RegisterFeedbackItemMap()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(FeedbackItem)))
        {
            return;
        }

        BsonClassMap.RegisterClassMap<FeedbackItem>(map =>
        {
            map.AutoMap();
            map.MapIdMember(x => x.Id).SetIdGenerator(CombGuidGenerator.Instance);
            map.GetMemberMap(x => x.CreatedAt)
                .SetSerializer(new DateTimeOffsetSerializer(BsonType.DateTime));
            map.GetMemberMap(x => x.UpdatedAt)
                .SetSerializer(new NullableSerializer<DateTimeOffset>(
                    new DateTimeOffsetSerializer(BsonType.DateTime)));
        });
    }

    private static void RegisterActivityEntryMap()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(ActivityEntry)))
        {
            return;
        }

        BsonClassMap.RegisterClassMap<ActivityEntry>(map =>
        {
            map.AutoMap();
            map.MapIdMember(x => x.Id).SetIdGenerator(CombGuidGenerator.Instance);
            map.GetMemberMap(x => x.Kind)
                .SetSerializer(new EnumSerializer<ActivityKind>(BsonType.String));
            map.GetMemberMap(x => x.OccurredAt)
                .SetSerializer(new DateTimeOffsetSerializer(BsonType.DateTime));
        });
    }

    public MongoDbContext(IOptions<MongoDbSettings> options)
    {
        var settings = options.Value;
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);

        FeedbackItems = database.GetCollection<FeedbackItem>(settings.FeedbackCollectionName);
        Activity = database.GetCollection<ActivityEntry>(settings.ActivityCollectionName);
    }

    public IMongoCollection<FeedbackItem> FeedbackItems { get; }

    public IMongoCollection<ActivityEntry> Activity { get; }

    public async Task CreateIndexesAsync(CancellationToken cancellationToken = default)
    {
        var keys = Builders<FeedbackItem>.IndexKeys;

        var byIdea = new CreateIndexModel<FeedbackItem>(
            keys.Ascending(x => x.IdeaId).Descending(x => x.CreatedAt));

        var byUser = new CreateIndexModel<FeedbackItem>(
            keys.Ascending(x => x.UserId).Descending(x => x.CreatedAt));

        var byCreatedAt = new CreateIndexModel<FeedbackItem>(
            keys.Descending(x => x.CreatedAt));

        var uniqueLike = new CreateIndexModel<FeedbackItem>(
            keys.Ascending(x => x.UserId).Ascending(x => x.IdeaId),
            new CreateIndexOptions<FeedbackItem>
            {
                Unique = true,
                PartialFilterExpression = new BsonDocumentFilterDefinition<FeedbackItem>(
                    new BsonDocument(nameof(FeedbackItem.Text), BsonNull.Value))
            });

        await FeedbackItems.Indexes.CreateManyAsync(
            [byIdea, byUser, byCreatedAt, uniqueLike],
            cancellationToken);

        await CreateActivityIndexesAsync(cancellationToken);
    }

    private async Task CreateActivityIndexesAsync(CancellationToken cancellationToken)
    {
        var keys = Builders<ActivityEntry>.IndexKeys;

        var uniqueKey = new CreateIndexModel<ActivityEntry>(
            keys.Ascending(x => x.Key),
            new CreateIndexOptions { Unique = true });

        var byCity = new CreateIndexModel<ActivityEntry>(
            keys.Ascending(x => x.CityId).Descending(x => x.OccurredAt).Descending(x => x.Id));

        var byOccurredAt = new CreateIndexModel<ActivityEntry>(
            keys.Descending(x => x.OccurredAt).Descending(x => x.Id));

        var byIdea = new CreateIndexModel<ActivityEntry>(keys.Ascending(x => x.IdeaId));

        var byPoll = new CreateIndexModel<ActivityEntry>(keys.Ascending(x => x.PollId));

        var byUser = new CreateIndexModel<ActivityEntry>(keys.Ascending(x => x.UserId));

        await Activity.Indexes.CreateManyAsync(
            [uniqueKey, byCity, byOccurredAt, byIdea, byPoll, byUser],
            cancellationToken);
    }
}
