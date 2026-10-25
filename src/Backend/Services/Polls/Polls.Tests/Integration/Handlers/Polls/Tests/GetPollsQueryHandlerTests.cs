using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Polls.Domain.Authorization;
using Polls.Domain.Cities;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.Infrastructure.Constants;
using Polls.Tests.Integration.Handlers.Cities;
using Polls.Tests.Integration.Handlers.Polls.TestData;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Polls.Tests;

public sealed class GetPollsQueryHandlerTests(PollsWebAppFactory factory) : PollsApiTestBase(factory)
{
    private const string FirstTitle = "Park renovation";
    private const string SecondTitle = "Bike lanes";
    private const string DescribedTitle = "Library hours";
    private const string DescriptionText = "Open on Sundays";
    private const string FirstImageFileName = "first.jpg";
    private const string SecondImageFileName = "second.jpg";
    private const int TotalPolls = 5;
    private const int PageSize = 2;
    private const int LastPage = 3;
    private const int SoonDays = 5;
    private const int LaterDays = 20;
    private const int CutoffDays = 10;
    private const string ItemsProperty = "items";
    private const string TitleProperty = "title";
    private const string IdProperty = "id";
    private const string ImagesProperty = "images";
    private const string UrlProperty = "url";
    private const string TotalCountProperty = "totalCount";
    private const string PageProperty = "page";
    private const string HasNextPageProperty = "hasNextPage";

    private async Task<City> SeedCityAsync()
    {
        var city = CityIntegrationHelpers.BuildCity();
        await SeedAsync(city);

        return city;
    }

    private static Poll BuildPoll(
        Guid cityId,
        string? title = null,
        PollStatus status = PollStatus.Active,
        PollType type = PollType.UserVote,
        params string[] imageFileNames)
    {
        var poll = PollIntegrationHelpers.BuildPoll(cityId, status, type, imageFileNames);
        poll.Title = title ?? poll.Id.ToString();

        return poll;
    }

    private async Task<(HttpStatusCode Status, JsonElement Value)> GetAsync(
        string route,
        string[] permissions,
        string query = "")
    {
        var response = await CreateClient(permissions).GetAsync($"{route}?{query}");
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            return (response.StatusCode, default);

        return (response.StatusCode, JsonDocument.Parse(body).RootElement);
    }

    private Task<(HttpStatusCode Status, JsonElement Value)> GetAsAdminAsync(string query = "") =>
        GetAsync(PollIntegrationHelpers.AdminPollsRoute, [Permissions.Polls.ReadAny], query);

    private Task<(HttpStatusCode Status, JsonElement Value)> GetAsUserAsync(string query = "") =>
        GetAsync(PollIntegrationHelpers.UserPollsRoute, [], query);

    private static string[] Titles(JsonElement value) =>
        value
            .GetProperty(ItemsProperty)
            .EnumerateArray()
            .Select(item => item.GetProperty(TitleProperty).GetString()!)
            .ToArray();

    private const int OldestAgeDays = 3;
    private const int MiddleAgeDays = 2;
    private const int NewestAgeDays = 1;

    private Task SetCreatedAtAsync(Guid id, DateTimeOffset createdAt) =>
        ExecuteDbAsync(context => context.Polls
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.CreatedAt, createdAt)));

    private static Guid[] Ids(JsonElement value) =>
        value
            .GetProperty(ItemsProperty)
            .EnumerateArray()
            .Select(item => item.GetProperty(IdProperty).GetGuid())
            .ToArray();

    [Fact]
    public async Task Handle_NoPolls_ReturnsEmptyList()
    {
        var (status, value) = await GetAsAdminAsync();

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Empty(Titles(value));
        Assert.Equal(0, value.GetProperty(TotalCountProperty).GetInt32());
    }

    [Fact]
    public async Task Handle_AdminRequest_ReturnsPollsOfAllStatuses()
    {
        var city = await SeedCityAsync();
        await SeedAsync(
            BuildPoll(city.Id, FirstTitle, PollStatus.Active),
            BuildPoll(city.Id, SecondTitle, PollStatus.Suspended),
            BuildPoll(city.Id, DescribedTitle, PollStatus.Finished));

        var (_, value) = await GetAsAdminAsync();

        Assert.Equivalent(new[] { FirstTitle, SecondTitle, DescribedTitle }, Titles(value));
    }

    [Fact]
    public async Task Handle_UserRequest_ReturnsOnlyActivePolls()
    {
        var city = await SeedCityAsync();
        await SeedAsync(
            BuildPoll(city.Id, FirstTitle, PollStatus.Active),
            BuildPoll(city.Id, SecondTitle, PollStatus.Suspended));

        var (_, value) = await GetAsUserAsync();

        Assert.Equal([FirstTitle], Titles(value));
    }

    [Fact]
    public async Task Handle_UserRequestAsksForOtherStatus_StillReturnsOnlyActivePolls()
    {
        var city = await SeedCityAsync();
        await SeedAsync(
            BuildPoll(city.Id, FirstTitle, PollStatus.Active),
            BuildPoll(city.Id, SecondTitle, PollStatus.Suspended));

        var (_, value) = await GetAsUserAsync($"Status={(int)PollStatus.Suspended}");

        Assert.Equal([FirstTitle], Titles(value));
    }

    [Fact]
    public async Task Handle_CityFilter_ReturnsOnlyPollsOfThatCity()
    {
        var city = await SeedCityAsync();
        var otherCity = await SeedCityAsync();
        await SeedAsync(BuildPoll(city.Id, FirstTitle), BuildPoll(otherCity.Id, SecondTitle));

        var (_, value) = await GetAsAdminAsync($"CityId={city.Id}");

        Assert.Equal([FirstTitle], Titles(value));
    }

    [Fact]
    public async Task Handle_TypeFilter_ReturnsOnlyPollsOfThatType()
    {
        var city = await SeedCityAsync();
        await SeedAsync(
            BuildPoll(city.Id, FirstTitle, type: PollType.UserVote),
            BuildPoll(city.Id, SecondTitle, type: PollType.ManagerVote));

        var (_, value) = await GetAsAdminAsync($"Type={(int)PollType.ManagerVote}");

        Assert.Equal([SecondTitle], Titles(value));
    }

    [Fact]
    public async Task Handle_StatusFilter_ReturnsOnlyPollsWithThatStatus()
    {
        var city = await SeedCityAsync();
        await SeedAsync(
            BuildPoll(city.Id, FirstTitle, PollStatus.Active),
            BuildPoll(city.Id, SecondTitle, PollStatus.Suspended));

        var (_, value) = await GetAsAdminAsync($"Status={(int)PollStatus.Suspended}");

        Assert.Equal([SecondTitle], Titles(value));
    }

    [Fact]
    public async Task Handle_SearchTerm_MatchesTitleIgnoringCase()
    {
        var city = await SeedCityAsync();
        await SeedAsync(BuildPoll(city.Id, FirstTitle), BuildPoll(city.Id, SecondTitle));

        var (_, value) = await GetAsAdminAsync($"SearchTerm={FirstTitle.ToUpperInvariant()}");

        Assert.Equal([FirstTitle], Titles(value));
    }

    [Fact]
    public async Task Handle_SearchTerm_MatchesDescription()
    {
        var city = await SeedCityAsync();
        var described = BuildPoll(city.Id, DescribedTitle);
        described.Description = DescriptionText;
        await SeedAsync(described, BuildPoll(city.Id, FirstTitle));

        var (_, value) = await GetAsAdminAsync($"SearchTerm={DescriptionText.ToLowerInvariant()}");

        Assert.Equal([DescribedTitle], Titles(value));
    }

    [Fact]
    public async Task Handle_EndsBeforeFilter_ReturnsOnlyPollsEndingEarlier()
    {
        var city = await SeedCityAsync();
        var soon = BuildPoll(city.Id, FirstTitle);
        soon.EndsAt = DateTimeOffset.UtcNow.AddDays(SoonDays);
        var later = BuildPoll(city.Id, SecondTitle);
        later.EndsAt = DateTimeOffset.UtcNow.AddDays(LaterDays);
        await SeedAsync(soon, later);
        var cutoff = Uri.EscapeDataString(DateTimeOffset.UtcNow.AddDays(CutoffDays).ToString("O"));

        var (_, value) = await GetAsAdminAsync($"EndsBefore={cutoff}");

        Assert.Equal([FirstTitle], Titles(value));
    }

    [Fact]
    public async Task Handle_Paging_ReturnsRequestedPageWithTotalCount()
    {
        var city = await SeedCityAsync();
        await SeedAsync(Enumerable
            .Range(0, TotalPolls)
            .Select(_ => (object)BuildPoll(city.Id))
            .ToArray());

        var (_, value) = await GetAsAdminAsync($"Page=2&PageSize={PageSize}");

        Assert.Equal(PageSize, Titles(value).Length);
        Assert.Equal(TotalPolls, value.GetProperty(TotalCountProperty).GetInt32());
        Assert.Equal(2, value.GetProperty(PageProperty).GetInt32());
        Assert.True(value.GetProperty(HasNextPageProperty).GetBoolean());
    }

    [Fact]
    public async Task Handle_PagingThroughAllPages_ReturnsEveryPollExactlyOnce()
    {
        var city = await SeedCityAsync();
        await SeedAsync(Enumerable
            .Range(0, TotalPolls)
            .Select(_ => (object)BuildPoll(city.Id))
            .ToArray());
        var ids = new List<Guid>();

        for (var page = 1; page <= LastPage; page++)
        {
            var (_, value) = await GetAsAdminAsync($"Page={page}&PageSize={PageSize}");
            ids.AddRange(value
                .GetProperty(ItemsProperty)
                .EnumerateArray()
                .Select(item => item.GetProperty(IdProperty).GetGuid()));
        }

        Assert.Equal(TotalPolls, ids.Count);
        Assert.Equal(TotalPolls, ids.Distinct().Count());
    }

    [Fact]
    public async Task Handle_Always_ReturnsNewestFirst()
    {
        var city = await SeedCityAsync();
        var oldest = BuildPoll(city.Id);
        var middle = BuildPoll(city.Id);
        var newest = BuildPoll(city.Id);
        await SeedAsync(oldest, middle, newest);
        var now = DateTimeOffset.UtcNow;
        await SetCreatedAtAsync(oldest.Id, now.AddDays(-OldestAgeDays));
        await SetCreatedAtAsync(middle.Id, now.AddDays(-MiddleAgeDays));
        await SetCreatedAtAsync(newest.Id, now.AddDays(-NewestAgeDays));

        var (_, value) = await GetAsAdminAsync();

        Assert.Equal([newest.Id, middle.Id, oldest.Id], Ids(value));
    }

    [Fact]
    public async Task Handle_ItemsCreatedAtSameMoment_OrdersThemById()
    {
        var city = await SeedCityAsync();
        var first = BuildPoll(city.Id);
        var second = BuildPoll(city.Id);
        var third = BuildPoll(city.Id);
        await SeedAsync(first, second, third);
        var createdAt = DateTimeOffset.UtcNow.AddDays(-NewestAgeDays);
        foreach (var item in new[] { first.Id, second.Id, third.Id })
            await SetCreatedAtAsync(item, createdAt);

        var (_, value) = await GetAsAdminAsync("IncludeImages=false");

        var expected = new[] { first.Id, second.Id, third.Id }.OrderBy(id => id.ToString(), StringComparer.Ordinal);
        Assert.Equal(expected, Ids(value));
    }

    [Fact]
    public async Task Handle_ImagesIncludedByDefault_ReturnsImageUrlsInOrder()
    {
        var city = await SeedCityAsync();
        await SeedAsync(BuildPoll(
            city.Id,
            FirstTitle,
            PollStatus.Active,
            PollType.UserVote,
            FirstImageFileName,
            SecondImageFileName));

        var (_, value) = await GetAsAdminAsync();

        var images = value.GetProperty(ItemsProperty)[0].GetProperty(ImagesProperty);
        Assert.Equal(
            [
                EnvironmentConstants.ImageStorage.UrlPrefix + FirstImageFileName,
                EnvironmentConstants.ImageStorage.UrlPrefix + SecondImageFileName
            ],
            images.EnumerateArray().Select(image => image.GetProperty(UrlProperty).GetString()));
    }

    [Fact]
    public async Task Handle_ImagesExcluded_ReturnsPollsWithoutImages()
    {
        var city = await SeedCityAsync();
        await SeedAsync(BuildPoll(
            city.Id,
            FirstTitle,
            PollStatus.Active,
            PollType.UserVote,
            FirstImageFileName));

        var (_, value) = await GetAsAdminAsync("IncludeImages=false");

        Assert.Equal(0, value.GetProperty(ItemsProperty)[0].GetProperty(ImagesProperty).GetArrayLength());
    }

    [Theory]
    [MemberData(nameof(GetPollsIntegrationTestData.InvalidFilters), MemberType = typeof(GetPollsIntegrationTestData))]
    public async Task Handle_FilterIsInvalid_ReturnsBadRequest(PollFilterQueryCase filter)
    {
        var (status, _) = await GetAsAdminAsync(filter.Query);

        Assert.Equal(HttpStatusCode.BadRequest, status);
    }
}
