using System.Text.RegularExpressions;
using CityPulse.Contracts.Querying.Pagination;
using ErrorOr;
using Feedback.API.Domain;
using Feedback.API.Infrastructure.Mongo;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Feedback.API.Features.Feedback.GetFilteredFeedback;

public class GetFilteredFeedbackQueryHandler(MongoDbContext context)
    : IRequestHandler<GetFilteredFeedbackQuery, ErrorOr<PagedList<FeedbackResponse>>>
{
    private const string CaseInsensitiveOption = "i";

    public async Task<ErrorOr<PagedList<FeedbackResponse>>> Handle(
        GetFilteredFeedbackQuery request,
        CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var definition = BuildFilter(filter);

        var totalCount = await context.FeedbackItems.CountDocumentsAsync(
            definition,
            cancellationToken: cancellationToken);

        var items = await context.FeedbackItems
            .Find(definition)
            .SortByDescending(x => x.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Limit(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<FeedbackResponse>(
            items.Select(FeedbackResponse.FromEntity).ToList(),
            filter.Page,
            filter.PageSize,
            (int)totalCount);
    }

    private static FilterDefinition<FeedbackItem> BuildFilter(FeedbackFilter filter)
    {
        var builder = Builders<FeedbackItem>.Filter;
        var conditions = new List<FilterDefinition<FeedbackItem>>();

        if (filter.IdeaId.HasValue)
            conditions.Add(builder.Eq(x => x.IdeaId, filter.IdeaId.Value));

        if (filter.UserId.HasValue)
            conditions.Add(builder.Eq(x => x.UserId, filter.UserId.Value));

        if (filter.Kind == FeedbackKind.Like)
            conditions.Add(builder.Eq(x => x.Text, null));

        if (filter.Kind == FeedbackKind.Comment)
            conditions.Add(builder.Ne(x => x.Text, null));

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var pattern = Regex.Escape(filter.SearchTerm.Trim());
            conditions.Add(builder.Regex(
                x => x.Text,
                new BsonRegularExpression(pattern, CaseInsensitiveOption)));
        }

        if (filter.CreatedFrom.HasValue)
            conditions.Add(builder.Gte(x => x.CreatedAt, filter.CreatedFrom.Value));

        if (filter.CreatedTo.HasValue)
            conditions.Add(builder.Lte(x => x.CreatedAt, filter.CreatedTo.Value));

        return conditions.Count == 0
            ? builder.Empty
            : builder.And(conditions);
    }
}
