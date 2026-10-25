using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Polls.Domain.Authorization;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Polls;
using Polls.Tests.Infrastructure;
using Polls.Tests.Infrastructure.Constants;
using Polls.Tests.Integration.Handlers.Cities;
using Polls.Tests.Integration.Handlers.Ideas.TestData;
using Polls.Tests.Integration.Handlers.Polls;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Ideas.Tests;

public sealed class GetIdeasQueryHandlerTests(PollsWebAppFactory factory) : PollsApiTestBase(factory)
{
    private const string FirstTitle = "More benches";
    private const string SecondTitle = "Playground";
    private const string ThirdTitle = "Fountain";
    private const string DescribedTitle = "Bike racks";
    private const string DescriptionText = "Near the station";
    private const string FirstImageFileName = "first.jpg";
    private const string SecondImageFileName = "second.jpg";
    private const int TotalIdeas = 5;
    private const int PageSize = 2;
    private const int LastPage = 3;
    private const string ValueProperty = "value";
    private const string ItemsProperty = "items";
    private const string TitleProperty = "title";
    private const string IdProperty = "id";
    private const string ImagesProperty = "images";
    private const string UrlProperty = "url";
    private const string TotalCountProperty = "totalCount";
    private const string PageProperty = "page";
    private const string HasNextPageProperty = "hasNextPage";

    private async Task<Poll> SeedPollAsync()
    {
        var city = CityIntegrationHelpers.BuildCity();
        var poll = PollIntegrationHelpers.BuildPoll(city.Id);
        await SeedAsync(city, poll);

        return poll;
    }

    private static Idea BuildIdea(
        Guid pollId,
        string? title = null,
        IdeaAccessStatus accessStatus = IdeaAccessStatus.Active,
        IdeaApprovalStatus approvalStatus = IdeaApprovalStatus.Pending,
        params string[] imageFileNames)
    {
        var idea = IdeaIntegrationHelpers.BuildIdea(pollId, null, accessStatus, approvalStatus, imageFileNames);
        idea.Title = title ?? idea.Id.ToString();

        return idea;
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

        return (response.StatusCode, JsonDocument.Parse(body).RootElement.GetProperty(ValueProperty));
    }

    private Task<(HttpStatusCode Status, JsonElement Value)> GetAsAdminAsync(string query = "") =>
        GetAsync(IdeaIntegrationHelpers.AdminIdeasRoute, [Permissions.Ideas.ReadAny], query);

    private Task<(HttpStatusCode Status, JsonElement Value)> GetAsUserAsync(string query = "") =>
        GetAsync(IdeaIntegrationHelpers.UserIdeasRoute, [], query);

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
        ExecuteDbAsync(context => context.Ideas
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.CreatedAt, createdAt)));

    private static Guid[] Ids(JsonElement value) =>
        value
            .GetProperty(ItemsProperty)
            .EnumerateArray()
            .Select(item => item.GetProperty(IdProperty).GetGuid())
            .ToArray();

    [Fact]
    public async Task Handle_NoIdeas_ReturnsEmptyList()
    {
        var (status, value) = await GetAsAdminAsync();

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Empty(Titles(value));
        Assert.Equal(0, value.GetProperty(TotalCountProperty).GetInt32());
    }

    [Fact]
    public async Task Handle_AdminRequest_ReturnsIdeasOfAllAccessStatuses()
    {
        var poll = await SeedPollAsync();
        await SeedAsync(
            BuildIdea(poll.Id, FirstTitle, IdeaAccessStatus.Active),
            BuildIdea(poll.Id, SecondTitle, IdeaAccessStatus.RestrictedByContext),
            BuildIdea(poll.Id, ThirdTitle, IdeaAccessStatus.RestrictedByAuthor));

        var (_, value) = await GetAsAdminAsync();

        Assert.Equivalent(new[] { FirstTitle, SecondTitle, ThirdTitle }, Titles(value));
    }

    [Fact]
    public async Task Handle_UserRequest_ReturnsOnlyIdeasWithActiveAccess()
    {
        var poll = await SeedPollAsync();
        await SeedAsync(
            BuildIdea(poll.Id, FirstTitle, IdeaAccessStatus.Active),
            BuildIdea(poll.Id, SecondTitle, IdeaAccessStatus.RestrictedByContext));

        var (_, value) = await GetAsUserAsync();

        Assert.Equal([FirstTitle], Titles(value));
    }

    [Fact]
    public async Task Handle_UserRequestAsksForOtherAccessStatus_StillReturnsOnlyActiveIdeas()
    {
        var poll = await SeedPollAsync();
        await SeedAsync(
            BuildIdea(poll.Id, FirstTitle, IdeaAccessStatus.Active),
            BuildIdea(poll.Id, SecondTitle, IdeaAccessStatus.RestrictedByContext));

        var (_, value) = await GetAsUserAsync($"AccessStatus={(int)IdeaAccessStatus.RestrictedByContext}");

        Assert.Equal([FirstTitle], Titles(value));
    }

    [Fact]
    public async Task Handle_PollFilter_ReturnsOnlyIdeasOfThatPoll()
    {
        var poll = await SeedPollAsync();
        var otherPoll = await SeedPollAsync();
        await SeedAsync(BuildIdea(poll.Id, FirstTitle), BuildIdea(otherPoll.Id, SecondTitle));

        var (_, value) = await GetAsAdminAsync($"PollId={poll.Id}");

        Assert.Equal([FirstTitle], Titles(value));
    }

    [Fact]
    public async Task Handle_AccessStatusFilter_ReturnsOnlyIdeasWithThatStatus()
    {
        var poll = await SeedPollAsync();
        await SeedAsync(
            BuildIdea(poll.Id, FirstTitle, IdeaAccessStatus.Active),
            BuildIdea(poll.Id, SecondTitle, IdeaAccessStatus.RestrictedByAuthor));

        var (_, value) = await GetAsAdminAsync($"AccessStatus={(int)IdeaAccessStatus.RestrictedByAuthor}");

        Assert.Equal([SecondTitle], Titles(value));
    }

    [Fact]
    public async Task Handle_ApprovalStatusFilter_ReturnsOnlyIdeasWithThatStatus()
    {
        var poll = await SeedPollAsync();
        await SeedAsync(
            BuildIdea(poll.Id, FirstTitle, approvalStatus: IdeaApprovalStatus.Pending),
            BuildIdea(poll.Id, SecondTitle, approvalStatus: IdeaApprovalStatus.Approved));

        var (_, value) = await GetAsAdminAsync($"ApprovalStatus={(int)IdeaApprovalStatus.Approved}");

        Assert.Equal([SecondTitle], Titles(value));
    }

    [Fact]
    public async Task Handle_SearchTerm_MatchesTitleIgnoringCase()
    {
        var poll = await SeedPollAsync();
        await SeedAsync(BuildIdea(poll.Id, FirstTitle), BuildIdea(poll.Id, SecondTitle));

        var (_, value) = await GetAsAdminAsync($"SearchTerm={FirstTitle.ToUpperInvariant()}");

        Assert.Equal([FirstTitle], Titles(value));
    }

    [Fact]
    public async Task Handle_SearchTerm_MatchesDescription()
    {
        var poll = await SeedPollAsync();
        var described = BuildIdea(poll.Id, DescribedTitle);
        described.Description = DescriptionText;
        await SeedAsync(described, BuildIdea(poll.Id, FirstTitle));

        var (_, value) = await GetAsAdminAsync($"SearchTerm={DescriptionText.ToLowerInvariant()}");

        Assert.Equal([DescribedTitle], Titles(value));
    }

    [Fact]
    public async Task Handle_Paging_ReturnsRequestedPageWithTotalCount()
    {
        var poll = await SeedPollAsync();
        await SeedAsync(Enumerable
            .Range(0, TotalIdeas)
            .Select(_ => (object)BuildIdea(poll.Id))
            .ToArray());

        var (_, value) = await GetAsAdminAsync($"Page=2&PageSize={PageSize}");

        Assert.Equal(PageSize, Titles(value).Length);
        Assert.Equal(TotalIdeas, value.GetProperty(TotalCountProperty).GetInt32());
        Assert.Equal(2, value.GetProperty(PageProperty).GetInt32());
        Assert.True(value.GetProperty(HasNextPageProperty).GetBoolean());
    }

    [Fact]
    public async Task Handle_PagingThroughAllPages_ReturnsEveryIdeaExactlyOnce()
    {
        var poll = await SeedPollAsync();
        await SeedAsync(Enumerable
            .Range(0, TotalIdeas)
            .Select(_ => (object)BuildIdea(poll.Id))
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

        Assert.Equal(TotalIdeas, ids.Count);
        Assert.Equal(TotalIdeas, ids.Distinct().Count());
    }

    [Fact]
    public async Task Handle_Always_ReturnsNewestFirst()
    {
        var poll = await SeedPollAsync();
        var oldest = BuildIdea(poll.Id);
        var middle = BuildIdea(poll.Id);
        var newest = BuildIdea(poll.Id);
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
        var poll = await SeedPollAsync();
        var first = BuildIdea(poll.Id);
        var second = BuildIdea(poll.Id);
        var third = BuildIdea(poll.Id);
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
        var poll = await SeedPollAsync();
        await SeedAsync(BuildIdea(
            poll.Id,
            FirstTitle,
            IdeaAccessStatus.Active,
            IdeaApprovalStatus.Pending,
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
    public async Task Handle_ImagesExcluded_ReturnsIdeasWithoutImages()
    {
        var poll = await SeedPollAsync();
        await SeedAsync(BuildIdea(
            poll.Id,
            FirstTitle,
            IdeaAccessStatus.Active,
            IdeaApprovalStatus.Pending,
            FirstImageFileName));

        var (_, value) = await GetAsAdminAsync("IncludeImages=false");

        Assert.Equal(0, value.GetProperty(ItemsProperty)[0].GetProperty(ImagesProperty).GetArrayLength());
    }

    [Theory]
    [MemberData(nameof(GetIdeasIntegrationTestData.InvalidFilters), MemberType = typeof(GetIdeasIntegrationTestData))]
    public async Task Handle_FilterIsInvalid_ReturnsBadRequest(IdeaFilterQueryCase filter)
    {
        var (status, _) = await GetAsAdminAsync(filter.Query);

        Assert.Equal(HttpStatusCode.BadRequest, status);
    }
}
