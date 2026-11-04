using ErrorOr;
using Feedback.API.Domain;
using Feedback.API.Infrastructure.Mongo;
using MediatR;
using MongoDB.Driver;

namespace Feedback.API.Features.Activity.GetActivity;

public class GetActivityQueryHandler(MongoDbContext context)
    : IRequestHandler<GetActivityQuery, ErrorOr<ActivityPage>>
{
    private static readonly FilterDefinitionBuilder<ActivityEntry> Filter = Builders<ActivityEntry>.Filter;

    public async Task<ErrorOr<ActivityPage>> Handle(
        GetActivityQuery request,
        CancellationToken cancellationToken)
    {
        var filter = request.Filter;

        var entries = await context.Activity
            .Find(BuildFilter(filter))
            .SortByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.Id)
            .Limit(filter.PageSize + 1)
            .ToListAsync(cancellationToken);

        var page = entries.Take(filter.PageSize).ToList();
        var nextCursor = entries.Count > filter.PageSize
            ? new ActivityCursor(page[^1].OccurredAt, page[^1].Id).Encode()
            : null;

        return new ActivityPage(page.Select(ActivityResponse.FromEntity).ToList(), nextCursor);
    }

    private static FilterDefinition<ActivityEntry> BuildFilter(ActivityFilter filter)
    {
        var conditions = new List<FilterDefinition<ActivityEntry>>
        {
            Filter.Eq(x => x.PollHidden, false),
            Filter.Eq(x => x.IdeaHidden, false),
            Filter.Eq(x => x.UserHidden, false)
        };

        if (filter.CityId.HasValue)
            conditions.Add(Filter.Eq(x => x.CityId, filter.CityId.Value));

        if (ActivityCursor.TryDecode(filter.Cursor, out var cursor))
            conditions.Add(Filter.Or(
                Filter.Lt(x => x.OccurredAt, cursor.OccurredAt),
                Filter.And(
                    Filter.Eq(x => x.OccurredAt, cursor.OccurredAt),
                    Filter.Lt(x => x.Id, cursor.Id))));

        return Filter.And(conditions);
    }
}
