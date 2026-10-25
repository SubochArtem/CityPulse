using Feedback.API.Infrastructure.Mongo;
using Feedback.Tests.Infrastructure.Constants;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Testcontainers.MongoDb;
using Xunit;

namespace Feedback.Tests.Infrastructure;

public sealed class MongoFixture : IAsyncLifetime
{
    private readonly MongoDbContainer _container = new MongoDbBuilder(EnvironmentConstants.Mongo.Image).Build();

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public TestMongo CreateIsolated()
    {
        var connectionString = _container.GetConnectionString();
        var databaseName = $"{EnvironmentConstants.Mongo.DatabasePrefix}{Guid.NewGuid():N}";

        var settings = new MongoDbSettings
        {
            ConnectionString = connectionString,
            DatabaseName = databaseName,
            FeedbackCollectionName = EnvironmentConstants.Mongo.FeedbackCollection
        };

        var rawCollection = new MongoClient(connectionString)
            .GetDatabase(databaseName)
            .GetCollection<BsonDocument>(EnvironmentConstants.Mongo.FeedbackCollection);

        return new TestMongo(new MongoDbContext(Options.Create(settings)), rawCollection);
    }
}

public sealed record TestMongo(MongoDbContext Context, IMongoCollection<BsonDocument> RawItems);

[CollectionDefinition(Name)]
public sealed class MongoCollection : ICollectionFixture<MongoFixture>
{
    public const string Name = "FeedbackMongo";
}
