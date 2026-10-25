using Feedback.API.Common.Interfaces;
using Feedback.API.Domain;
using Feedback.API.Infrastructure.Mongo;
using Feedback.Tests.Infrastructure.Constants;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace Feedback.Tests.Infrastructure;

[Collection(MongoCollection.Name)]
[Trait(EnvironmentConstants.Traits.Category, EnvironmentConstants.Traits.Integration)]
public abstract class MongoTestsBase
{
    private readonly TestMongo _mongo;

    protected MongoTestsBase(MongoFixture fixture)
    {
        _mongo = fixture.CreateIsolated();
        DateTimeProvider.Setup(x => x.UtcNow).Returns(Now);
    }

    protected static DateTimeOffset Now => FeedbackItems.DefaultCreatedAt.AddDays(1);

    protected MongoDbContext Context => _mongo.Context;

    protected IMongoCollection<BsonDocument> RawItems => _mongo.RawItems;

    protected Mock<IDateTimeProvider> DateTimeProvider { get; } = new(MockBehavior.Strict);

    protected Mock<IIdeaService> IdeaService { get; } = new(MockBehavior.Strict);

    protected Task SeedAsync(params FeedbackItem[] items) =>
        Context.FeedbackItems.InsertManyAsync(items);

    protected Task<List<FeedbackItem>> ReadAllAsync() =>
        Context.FeedbackItems.Find(FilterDefinition<FeedbackItem>.Empty).ToListAsync();

    protected Task<FeedbackItem?> FindAsync(Guid id) =>
        Context.FeedbackItems.Find(x => x.Id == id).FirstOrDefaultAsync()!;
}
