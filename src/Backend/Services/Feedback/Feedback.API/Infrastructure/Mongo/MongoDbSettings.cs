namespace Feedback.API.Infrastructure.Mongo;

public class MongoDbSettings
{
    public const string SectionName = "MongoDb";
    public required string ConnectionString { get; set; }
    public required string DatabaseName { get; set; }
    public required string FeedbackCollectionName { get; set; }
}
