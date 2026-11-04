using Feedback.API.Domain;
using Feedback.API.Features.Activity.GetActivity;
using Feedback.Tests.Infrastructure;
using Xunit;

namespace Feedback.Tests.Integration.Activity.Tests;

public sealed class GetActivityQueryHandlerTests : MongoTestsBase
{
    private readonly Guid _cityId = Guid.NewGuid();
    private readonly GetActivityQueryHandler _sut;

    public GetActivityQueryHandlerTests(MongoFixture fixture) : base(fixture)
    {
        _sut = new GetActivityQueryHandler(Context);
    }

    private async Task<ActivityPage> GetAsync(Guid? cityId = null, string? cursor = null, int pageSize = 20)
    {
        var result = await _sut.Handle(
            new GetActivityQuery(new ActivityFilter { CityId = cityId, Cursor = cursor, PageSize = pageSize }),
            CancellationToken.None);

        Assert.False(result.IsError);

        return result.Value;
    }

    private static DateTimeOffset At(int minutes) => ActivityEntries.DefaultOccurredAt.AddMinutes(minutes);

    [Fact]
    public async Task Handle_NoEntries_ReturnsEmptyPageWithoutCursor()
    {
        var page = await GetAsync();

        Assert.Empty(page.Items);
        Assert.Null(page.NextCursor);
    }

    [Fact]
    public async Task Handle_EntriesExist_ReturnsNewestFirst()
    {
        var oldest = ActivityEntries.IdeaProposed(_cityId, occurredAt: At(1));
        var newest = ActivityEntries.IdeaProposed(_cityId, occurredAt: At(3));
        var middle = ActivityEntries.PollOpened(_cityId, occurredAt: At(2));
        await SeedActivityAsync(oldest, newest, middle);

        var page = await GetAsync();

        Assert.Equal([newest.Id, middle.Id, oldest.Id], page.Items.Select(x => x.Id));
    }

    [Fact]
    public async Task Handle_CityGiven_ReturnsOnlyThatCityEntries()
    {
        var own = ActivityEntries.IdeaProposed(_cityId);
        await SeedActivityAsync(own, ActivityEntries.IdeaProposed());

        var page = await GetAsync(_cityId);

        Assert.Equal(own.Id, Assert.Single(page.Items).Id);
    }

    [Fact]
    public async Task Handle_NoCityGiven_ReturnsEntriesOfAllCities()
    {
        await SeedActivityAsync(ActivityEntries.IdeaProposed(_cityId), ActivityEntries.IdeaProposed());

        var page = await GetAsync();

        Assert.Equal(2, page.Items.Count);
    }

    [Fact]
    public async Task Handle_HiddenEntries_AreExcluded()
    {
        var visible = ActivityEntries.IdeaProposed(_cityId);
        var pollHidden = ActivityEntries.IdeaProposed(_cityId);
        pollHidden.PollHidden = true;
        var ideaHidden = ActivityEntries.IdeaProposed(_cityId);
        ideaHidden.IdeaHidden = true;
        var userHidden = ActivityEntries.IdeaProposed(_cityId);
        userHidden.UserHidden = true;
        await SeedActivityAsync(visible, pollHidden, ideaHidden, userHidden);

        var page = await GetAsync(_cityId);

        Assert.Equal(visible.Id, Assert.Single(page.Items).Id);
    }

    [Fact]
    public async Task Handle_EntryFound_MapsAllFields()
    {
        var entry = ActivityEntries.IdeaProposed(_cityId);
        await SeedActivityAsync(entry);

        var item = Assert.Single((await GetAsync()).Items);

        Assert.Equal(entry.Id, item.Id);
        Assert.Equal(ActivityKind.IdeaProposed, item.Kind);
        Assert.Equal(entry.CityId, item.CityId);
        Assert.Equal(entry.PollId, item.PollId);
        Assert.Equal(entry.PollTitle, item.PollTitle);
        Assert.Equal(entry.IdeaId, item.IdeaId);
        Assert.Equal(entry.IdeaTitle, item.IdeaTitle);
        Assert.Equal(entry.UserId, item.UserId);
        Assert.Equal(entry.Status, item.Status);
        Assert.Equal(entry.OccurredAt, item.OccurredAt);
    }

    [Fact]
    public async Task Handle_MoreEntriesThanPageSize_ReturnsCursorThatContinuesWithoutGapsOrDuplicates()
    {
        var entries = Enumerable.Range(0, 7)
            .Select(i => ActivityEntries.IdeaProposed(_cityId, occurredAt: At(i)))
            .ToArray();
        await SeedActivityAsync(entries);

        var first = await GetAsync(_cityId, pageSize: 3);
        var second = await GetAsync(_cityId, first.NextCursor, 3);
        var third = await GetAsync(_cityId, second.NextCursor, 3);

        var expected = entries.OrderByDescending(x => x.OccurredAt).Select(x => x.Id).ToList();
        Assert.Equal(expected, first.Items.Concat(second.Items).Concat(third.Items).Select(x => x.Id));
        Assert.NotNull(first.NextCursor);
        Assert.NotNull(second.NextCursor);
        Assert.Null(third.NextCursor);
    }

    [Fact]
    public async Task Handle_EntriesShareTimestamp_PagesThroughAllOfThemOnce()
    {
        var entries = Enumerable.Range(0, 5)
            .Select(_ => ActivityEntries.IdeaProposed(_cityId, occurredAt: At(0)))
            .ToArray();
        await SeedActivityAsync(entries);

        var seen = new List<Guid>();
        string? cursor = null;
        do
        {
            var page = await GetAsync(_cityId, cursor, 2);
            seen.AddRange(page.Items.Select(x => x.Id));
            cursor = page.NextCursor;
        } while (cursor is not null);

        Assert.Equal(entries.Select(x => x.Id).Order(), seen.Order());
        Assert.Equal(seen.Count, seen.Distinct().Count());
    }

    [Fact]
    public async Task Handle_ExactlyPageSizeEntries_ReturnsNoCursor()
    {
        await SeedActivityAsync(
            ActivityEntries.IdeaProposed(_cityId, occurredAt: At(0)),
            ActivityEntries.IdeaProposed(_cityId, occurredAt: At(1)));

        var page = await GetAsync(_cityId, pageSize: 2);

        Assert.Equal(2, page.Items.Count);
        Assert.Null(page.NextCursor);
    }
}
