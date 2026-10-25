using Feedback.API.Domain;
using Feedback.API.Features.Feedback.GetFilteredFeedback;
using Feedback.Tests.Infrastructure;
using Feedback.Tests.Integration.Handlers.TestData;
using Xunit;

namespace Feedback.Tests.Integration.Handlers.Tests;

public sealed class GetFilteredFeedbackQueryHandlerTests(MongoFixture fixture) : MongoTestsBase(fixture)
{
    private const string GreatIdeaText = "Great idea for the park";
    private const string OtherText = "Please fix the road";
    private const string GreatSearchTerm = "great";
    private const string SequentialTextPrefix = "comment";
    private const string LiteralVersionText = "version a.b released";
    private const string LookalikeVersionText = "version axb released";
    private const string RegexSearchTerm = "a.b";
    private const int TotalItems = 25;
    private const int PageSize = 10;
    private const int UnknownKind = 99;

    private static readonly DateTimeOffset BaseTime = FeedbackItems.DefaultCreatedAt;

    private GetFilteredFeedbackQueryHandler CreateSut() => new(Context);

    private async Task<(List<Guid> Ids, int TotalCount, PagedFlags Flags)> QueryAsync(FeedbackFilter filter)
    {
        var result = await CreateSut().Handle(new GetFilteredFeedbackQuery(filter), CancellationToken.None);

        Assert.False(result.IsError);

        return (
            result.Value.Items.Select(item => item.Id).ToList(),
            result.Value.TotalCount,
            new PagedFlags(result.Value.HasNextPage, result.Value.HasPreviousPage));
    }

    private async Task<List<FeedbackItem>> SeedSequentialAsync(int count, Guid? ideaId = null, Guid? userId = null)
    {
        var items = Enumerable
            .Range(0, count)
            .Select(index => FeedbackItems.Comment(
                userId,
                ideaId,
                $"{SequentialTextPrefix} {index}",
                BaseTime.AddMinutes(index)))
            .ToList();

        await SeedAsync(items.ToArray());

        return items;
    }

    private static List<Guid> NewestFirst(IEnumerable<FeedbackItem> items) =>
        items.OrderByDescending(item => item.CreatedAt).Select(item => item.Id).ToList();

    [Fact]
    public async Task Handle_NothingStored_ReturnsEmptyPage()
    {
        var (ids, totalCount, flags) = await QueryAsync(new FeedbackFilter());

        Assert.Empty(ids);
        Assert.Equal(0, totalCount);
        Assert.False(flags.HasNextPage);
        Assert.False(flags.HasPreviousPage);
    }

    [Fact]
    public async Task Handle_NoFilters_ReturnsAllItemsNewestFirst()
    {
        var oldest = FeedbackItems.Comment(createdAt: BaseTime);
        var middle = FeedbackItems.Like(createdAt: BaseTime.AddHours(1));
        var newest = FeedbackItems.Comment(createdAt: BaseTime.AddHours(2));
        await SeedAsync(middle, newest, oldest);

        var (ids, totalCount, _) = await QueryAsync(new FeedbackFilter());

        Assert.Equal([newest.Id, middle.Id, oldest.Id], ids);
        Assert.Equal(3, totalCount);
    }

    [Fact]
    public async Task Handle_SecondPage_ReturnsMiddleItemsWithPagingFlags()
    {
        var items = await SeedSequentialAsync(TotalItems);
        var expected = NewestFirst(items).Skip(PageSize).Take(PageSize).ToList();

        var (ids, totalCount, flags) = await QueryAsync(new FeedbackFilter { Page = 2, PageSize = PageSize });

        Assert.Equal(expected, ids);
        Assert.Equal(TotalItems, totalCount);
        Assert.True(flags.HasNextPage);
        Assert.True(flags.HasPreviousPage);
    }

    [Fact]
    public async Task Handle_LastIncompletePage_ReturnsRemainingItemsWithoutNextPage()
    {
        var items = await SeedSequentialAsync(TotalItems);
        var expected = NewestFirst(items).Skip(PageSize * 2).ToList();

        var (ids, totalCount, flags) = await QueryAsync(new FeedbackFilter { Page = 3, PageSize = PageSize });

        Assert.Equal(expected, ids);
        Assert.Equal(TotalItems, totalCount);
        Assert.False(flags.HasNextPage);
        Assert.True(flags.HasPreviousPage);
    }

    [Fact]
    public async Task Handle_PageBeyondTheEnd_ReturnsNoItemsButKeepsTotalCount()
    {
        await SeedSequentialAsync(TotalItems);

        var (ids, totalCount, _) = await QueryAsync(new FeedbackFilter { Page = 4, PageSize = PageSize });

        Assert.Empty(ids);
        Assert.Equal(TotalItems, totalCount);
    }

    [Fact]
    public async Task Handle_FilterByIdea_ReturnsOnlyItemsOfThatIdea()
    {
        var ideaId = Guid.NewGuid();
        var ofIdea = await SeedSequentialAsync(3, ideaId);
        await SeedSequentialAsync(2);

        var (ids, totalCount, _) = await QueryAsync(new FeedbackFilter { IdeaId = ideaId });

        Assert.Equal(NewestFirst(ofIdea), ids);
        Assert.Equal(3, totalCount);
    }

    [Fact]
    public async Task Handle_FilterByUser_ReturnsOnlyItemsOfThatUser()
    {
        var userId = Guid.NewGuid();
        var ofUser = await SeedSequentialAsync(2, userId: userId);
        await SeedSequentialAsync(3);

        var (ids, totalCount, _) = await QueryAsync(new FeedbackFilter { UserId = userId });

        Assert.Equal(NewestFirst(ofUser), ids);
        Assert.Equal(2, totalCount);
    }

    [Fact]
    public async Task Handle_FilterByLikeKind_ReturnsOnlyLikes()
    {
        var like = FeedbackItems.Like(createdAt: BaseTime.AddMinutes(1));
        await SeedAsync(like, FeedbackItems.Comment(createdAt: BaseTime.AddMinutes(2)));

        var (ids, totalCount, _) = await QueryAsync(new FeedbackFilter { Kind = FeedbackKind.Like });

        Assert.Equal([like.Id], ids);
        Assert.Equal(1, totalCount);
    }

    [Fact]
    public async Task Handle_FilterByCommentKind_ReturnsOnlyComments()
    {
        var comment = FeedbackItems.Comment(createdAt: BaseTime.AddMinutes(2));
        await SeedAsync(FeedbackItems.Like(createdAt: BaseTime.AddMinutes(1)), comment);

        var (ids, totalCount, _) = await QueryAsync(new FeedbackFilter { Kind = FeedbackKind.Comment });

        Assert.Equal([comment.Id], ids);
        Assert.Equal(1, totalCount);
    }

    [Fact]
    public async Task Handle_UnknownKind_DoesNotFilterByKind()
    {
        await SeedAsync(FeedbackItems.Like(), FeedbackItems.Comment());

        var (_, totalCount, _) = await QueryAsync(new FeedbackFilter { Kind = (FeedbackKind)UnknownKind });

        Assert.Equal(2, totalCount);
    }

    [Theory]
    [MemberData(nameof(GetFilteredFeedbackTestData.MatchingSearchTerms), MemberType = typeof(GetFilteredFeedbackTestData))]
    public async Task Handle_SearchTermMatchesText_ReturnsItIgnoringCaseAndSurroundingSpaces(string searchTerm)
    {
        var match = FeedbackItems.Comment(text: GreatIdeaText);
        await SeedAsync(match, FeedbackItems.Comment(text: OtherText));

        var (ids, _, _) = await QueryAsync(new FeedbackFilter { SearchTerm = searchTerm });

        Assert.Equal([match.Id], ids);
    }

    [Fact]
    public async Task Handle_SearchTermContainsRegexCharacters_MatchesThemLiterally()
    {
        var literal = FeedbackItems.Comment(text: LiteralVersionText);
        await SeedAsync(literal, FeedbackItems.Comment(text: LookalikeVersionText));

        var (ids, _, _) = await QueryAsync(new FeedbackFilter { SearchTerm = RegexSearchTerm });

        Assert.Equal([literal.Id], ids);
    }

    [Fact]
    public async Task Handle_SearchTermIsSet_ExcludesLikes()
    {
        var comment = FeedbackItems.Comment(text: GreatIdeaText);
        await SeedAsync(comment, FeedbackItems.Like());

        var (ids, totalCount, _) = await QueryAsync(new FeedbackFilter { SearchTerm = GreatSearchTerm });

        Assert.Equal([comment.Id], ids);
        Assert.Equal(1, totalCount);
    }

    [Theory]
    [MemberData(nameof(GetFilteredFeedbackTestData.IgnoredSearchTerms), MemberType = typeof(GetFilteredFeedbackTestData))]
    public async Task Handle_SearchTermIsBlank_IgnoresIt(string searchTerm)
    {
        await SeedAsync(FeedbackItems.Comment(), FeedbackItems.Like());

        var (_, totalCount, _) = await QueryAsync(new FeedbackFilter { SearchTerm = searchTerm });

        Assert.Equal(2, totalCount);
    }

    [Fact]
    public async Task Handle_CreatedFromIsSet_IncludesItemsCreatedExactlyAtThatTime()
    {
        var items = await SeedAtDaysAsync(0, 1, 2);

        var (ids, _, _) = await QueryAsync(new FeedbackFilter { CreatedFrom = BaseTime.AddDays(1) });

        Assert.Equal(NewestFirst(items.Skip(1)), ids);
    }

    [Fact]
    public async Task Handle_CreatedToIsSet_IncludesItemsCreatedExactlyAtThatTime()
    {
        var items = await SeedAtDaysAsync(0, 1, 2);

        var (ids, _, _) = await QueryAsync(new FeedbackFilter { CreatedTo = BaseTime.AddDays(1) });

        Assert.Equal(NewestFirst(items.Take(2)), ids);
    }

    [Fact]
    public async Task Handle_CreatedFromEqualsCreatedTo_ReturnsItemsCreatedAtThatMoment()
    {
        var items = await SeedAtDaysAsync(0, 1, 2);
        var moment = BaseTime.AddDays(1);

        var (ids, _, _) = await QueryAsync(new FeedbackFilter { CreatedFrom = moment, CreatedTo = moment });

        Assert.Equal([items[1].Id], ids);
    }

    [Fact]
    public async Task Handle_SeveralFilters_ReturnsOnlyItemsMatchingAll()
    {
        var ideaId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var match = FeedbackItems.Comment(userId, ideaId, GreatIdeaText, BaseTime.AddHours(1));
        await SeedAsync(
            match,
            FeedbackItems.Comment(userId, ideaId, OtherText, BaseTime.AddHours(1)),
            FeedbackItems.Comment(Guid.NewGuid(), ideaId, GreatIdeaText, BaseTime.AddHours(1)),
            FeedbackItems.Comment(userId, Guid.NewGuid(), GreatIdeaText, BaseTime.AddHours(1)),
            FeedbackItems.Comment(userId, ideaId, GreatIdeaText, BaseTime.AddDays(5)),
            FeedbackItems.Like(userId, ideaId, BaseTime.AddHours(1)));
        var filter = new FeedbackFilter
        {
            IdeaId = ideaId,
            UserId = userId,
            Kind = FeedbackKind.Comment,
            SearchTerm = GreatSearchTerm,
            CreatedFrom = BaseTime,
            CreatedTo = BaseTime.AddDays(1)
        };

        var (ids, totalCount, _) = await QueryAsync(filter);

        Assert.Equal([match.Id], ids);
        Assert.Equal(1, totalCount);
    }

    [Fact]
    public async Task Handle_FilterAndPaging_CountsTotalOfFilteredItemsOnly()
    {
        var ideaId = Guid.NewGuid();
        await SeedSequentialAsync(5, ideaId);
        await SeedSequentialAsync(3);

        var (ids, totalCount, flags) = await QueryAsync(new FeedbackFilter { IdeaId = ideaId, PageSize = 2 });

        Assert.Equal(2, ids.Count);
        Assert.Equal(5, totalCount);
        Assert.True(flags.HasNextPage);
    }

    private async Task<List<FeedbackItem>> SeedAtDaysAsync(params int[] days)
    {
        var items = days
            .Select(day => FeedbackItems.Comment(createdAt: BaseTime.AddDays(day)))
            .ToList();

        await SeedAsync(items.ToArray());

        return items;
    }

    private sealed record PagedFlags(bool HasNextPage, bool HasPreviousPage);
}
