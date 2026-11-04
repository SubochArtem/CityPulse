using System.Linq.Expressions;
using Feedback.API.Domain;
using Feedback.API.Infrastructure.Mongo;
using MongoDB.Driver;

namespace Feedback.API.Features.Activity;

public class ActivityStore(MongoDbContext context)
{
    private static readonly FilterDefinitionBuilder<ActivityEntry> Filter = Builders<ActivityEntry>.Filter;
    private static readonly UpdateDefinitionBuilder<ActivityEntry> Update = Builders<ActivityEntry>.Update;

    public async Task<bool> AddAsync(ActivityEntry entry, CancellationToken cancellationToken = default)
    {
        entry.PollHidden = entry.PollId.HasValue
                           && await IsHiddenAsync(x => x.PollId, entry.PollId.Value, x => x.PollHidden, cancellationToken);
        entry.IdeaHidden = entry.IdeaId.HasValue
                           && await IsHiddenAsync(x => x.IdeaId, entry.IdeaId.Value, x => x.IdeaHidden, cancellationToken);
        entry.UserHidden = entry.UserId.HasValue
                           && await IsHiddenAsync(x => x.UserId, entry.UserId.Value, x => x.UserHidden, cancellationToken);

        try
        {
            await context.Activity.InsertOneAsync(entry, cancellationToken: cancellationToken);

            return true;
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            return false;
        }
    }

    public Task<ActivityEntry?> FindIdeaProposalAsync(Guid ideaId, CancellationToken cancellationToken = default) =>
        context.Activity
            .Find(x => x.Key == ActivityKeys.IdeaProposed(ideaId))
            .FirstOrDefaultAsync(cancellationToken)!;

    public Task SetPollHiddenAsync(Guid pollId, bool hidden, CancellationToken cancellationToken = default) =>
        context.Activity.UpdateManyAsync(
            Filter.Eq(x => x.PollId, pollId),
            Update.Set(x => x.PollHidden, hidden),
            cancellationToken: cancellationToken);

    public Task SetIdeaHiddenAsync(Guid ideaId, bool hidden, CancellationToken cancellationToken = default) =>
        context.Activity.UpdateManyAsync(
            Filter.Eq(x => x.IdeaId, ideaId),
            Update.Set(x => x.IdeaHidden, hidden),
            cancellationToken: cancellationToken);

    public Task SetUserHiddenAsync(Guid userId, bool hidden, CancellationToken cancellationToken = default) =>
        context.Activity.UpdateManyAsync(
            Filter.Eq(x => x.UserId, userId),
            Update.Set(x => x.UserHidden, hidden),
            cancellationToken: cancellationToken);

    public Task DeleteByPollAsync(Guid pollId, CancellationToken cancellationToken = default) =>
        context.Activity.DeleteManyAsync(Filter.Eq(x => x.PollId, pollId), cancellationToken);

    public Task DeleteByIdeaAsync(Guid ideaId, CancellationToken cancellationToken = default) =>
        context.Activity.DeleteManyAsync(Filter.Eq(x => x.IdeaId, ideaId), cancellationToken);

    public Task DeleteByKeyAsync(string key, CancellationToken cancellationToken = default) =>
        context.Activity.DeleteOneAsync(Filter.Eq(x => x.Key, key), cancellationToken);

    private Task<bool> IsHiddenAsync(
        Expression<Func<ActivityEntry, Guid?>> field,
        Guid value,
        Expression<Func<ActivityEntry, bool>> hiddenFlag,
        CancellationToken cancellationToken) =>
        context.Activity
            .Find(Filter.And(Filter.Eq(field, value), Filter.Eq(hiddenFlag, true)))
            .AnyAsync(cancellationToken);
}
