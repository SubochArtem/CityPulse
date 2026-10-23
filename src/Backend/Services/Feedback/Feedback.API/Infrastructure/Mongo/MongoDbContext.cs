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

    public MongoDbContext(IOptions<MongoDbSettings> options)
    {
        var settings = options.Value;
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);

        FeedbackItems = database.GetCollection<FeedbackItem>(settings.FeedbackCollectionName);
    }

    public IMongoCollection<FeedbackItem> FeedbackItems { get; }

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
    }
}
