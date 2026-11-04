using System.Linq.Expressions;
using Feedback.API.Domain;
using Feedback.API.Features.Activity.Live;
using Feedback.API.Infrastructure.Mongo;
using MongoDB.Driver;

namespace Feedback.API.Features.Activity;

public class ActivityStore(MongoDbContext context, IActivityNotifier notifier)
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
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            return false;
        }

        if (!entry.PollHidden && !entry.IdeaHidden && !entry.UserHidden)
            await notifier.NotifyAddedAsync(entry);

        return true;
    }

    public Task<ActivityEntry?> FindIdeaProposalAsync(Guid ideaId, CancellationToken cancellationToken = default) =>
        context.Activity
            .Find(x => x.Key == ActivityKeys.IdeaProposed(ideaId))
            .FirstOrDefaultAsync(cancellationToken)!;

    public Task SetPollHiddenAsync(
        Guid pollId,
        Guid cityId,
        bool hidden,
        CancellationToken cancellationToken = default) =>
        SetHiddenAsync(x => x.PollId, pollId, x => x.PollHidden, hidden, cityId, cancellationToken);

    public Task SetIdeaHiddenAsync(
        Guid ideaId,
        Guid cityId,
        bool hidden,
        CancellationToken cancellationToken = default) =>
        SetHiddenAsync(x => x.IdeaId, ideaId, x => x.IdeaHidden, hidden, cityId, cancellationToken);

    public Task SetUserHiddenAsync(Guid userId, bool hidden, CancellationToken cancellationToken = default) =>
        SetHiddenAsync(x => x.UserId, userId, x => x.UserHidden, hidden, null, cancellationToken);

    public Task DeleteByPollAsync(Guid pollId, Guid cityId, CancellationToken cancellationToken = default) =>
        DeleteManyAsync(Filter.Eq(x => x.PollId, pollId), cityId, cancellationToken);

    public Task DeleteByIdeaAsync(Guid ideaId, Guid cityId, CancellationToken cancellationToken = default) =>
        DeleteManyAsync(Filter.Eq(x => x.IdeaId, ideaId), cityId, cancellationToken);

    public async Task DeleteByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        var deleted = await context.Activity.FindOneAndDeleteAsync(
            Filter.Eq(x => x.Key, key),
            cancellationToken: cancellationToken);

        if (deleted is not null)
            await notifier.NotifyInvalidatedAsync(deleted.CityId);
    }

    private async Task SetHiddenAsync(
        Expression<Func<ActivityEntry, Guid?>> field,
        Guid value,
        Expression<Func<ActivityEntry, bool>> hiddenFlag,
        bool hidden,
        Guid? cityId,
        CancellationToken cancellationToken)
    {
        var result = await context.Activity.UpdateManyAsync(
            Filter.And(Filter.Eq(field, value), Filter.Ne(hiddenFlag, hidden)),
            Update.Set(hiddenFlag, hidden),
            cancellationToken: cancellationToken);

        if (result.ModifiedCount > 0)
            await notifier.NotifyInvalidatedAsync(cityId);
    }

    private async Task DeleteManyAsync(
        FilterDefinition<ActivityEntry> filter,
        Guid cityId,
        CancellationToken cancellationToken)
    {
        var result = await context.Activity.DeleteManyAsync(filter, cancellationToken);

        if (result.DeletedCount > 0)
            await notifier.NotifyInvalidatedAsync(cityId);
    }

    private Task<bool> IsHiddenAsync(
        Expression<Func<ActivityEntry, Guid?>> field,
        Guid value,
        Expression<Func<ActivityEntry, bool>> hiddenFlag,
        CancellationToken cancellationToken) =>
        context.Activity
            .Find(Filter.And(Filter.Eq(field, value), Filter.Eq(hiddenFlag, true)))
            .AnyAsync(cancellationToken);
}
