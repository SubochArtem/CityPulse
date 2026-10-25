using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Polls.Domain.Authorization;
using Polls.Domain.Cities.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.Infrastructure.Constants;
using Polls.Tests.Integration.Handlers.Cities.TestData;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Cities.Tests;

public sealed class GetCitiesQueryHandlerTests(PollsWebAppFactory factory) : PollsApiTestBase(factory)
{
    private const string ActiveTitle = "Springfield";
    private const string InactiveTitle = "Shelbyville";
    private const string DescribedTitle = "Capital City";
    private const string DescriptionText = "Known for its Monorail";
    private const string FirstImageFileName = "first.jpg";
    private const string SecondImageFileName = "second.jpg";
    private const int TotalCities = 5;
    private const int PageSize = 2;
    private const int LastPage = 3;
    private const string ItemsProperty = "items";
    private const string TitleProperty = "title";
    private const string IdProperty = "id";
    private const string ImagesProperty = "images";
    private const string UrlProperty = "url";
    private const string TotalCountProperty = "totalCount";
    private const string PageProperty = "page";
    private const string HasNextPageProperty = "hasNextPage";

    private async Task<(HttpStatusCode Status, JsonElement Value)> GetAsync(string query = "")
    {
        var response = await CreateClient([Permissions.Cities.ReadAny])
            .GetAsync($"{CityIntegrationHelpers.AdminCitiesRoute}?{query}");

        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            return (response.StatusCode, default);

        return (response.StatusCode, JsonDocument.Parse(body).RootElement);
    }

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
        ExecuteDbAsync(context => context.Cities
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.CreatedAt, createdAt)));

    private static Guid[] Ids(JsonElement value) =>
        value
            .GetProperty(ItemsProperty)
            .EnumerateArray()
            .Select(item => item.GetProperty(IdProperty).GetGuid())
            .ToArray();

    [Fact]
    public async Task Handle_NoCities_ReturnsEmptyList()
    {
        var (status, value) = await GetAsync();

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Empty(Titles(value));
        Assert.Equal(0, value.GetProperty(TotalCountProperty).GetInt32());
    }

    [Fact]
    public async Task Handle_Always_ReturnsCitiesOfAllStatuses()
    {
        await SeedAsync(
            CityIntegrationHelpers.BuildCity(ActiveTitle, CityStatus.Active),
            CityIntegrationHelpers.BuildCity(InactiveTitle, CityStatus.Inactive));

        var (_, value) = await GetAsync();

        Assert.Equivalent(new[] { ActiveTitle, InactiveTitle }, Titles(value));
    }

    [Fact]
    public async Task Handle_StatusFilter_ReturnsOnlyCitiesWithThatStatus()
    {
        await SeedAsync(
            CityIntegrationHelpers.BuildCity(ActiveTitle, CityStatus.Active),
            CityIntegrationHelpers.BuildCity(InactiveTitle, CityStatus.Inactive));

        var (_, value) = await GetAsync($"Status={(int)CityStatus.Inactive}");

        Assert.Equal([InactiveTitle], Titles(value));
    }

    [Fact]
    public async Task Handle_SearchTerm_MatchesTitleIgnoringCase()
    {
        await SeedAsync(
            CityIntegrationHelpers.BuildCity(ActiveTitle),
            CityIntegrationHelpers.BuildCity(InactiveTitle));

        var (_, value) = await GetAsync($"SearchTerm={ActiveTitle.ToUpperInvariant()}");

        Assert.Equal([ActiveTitle], Titles(value));
    }

    [Fact]
    public async Task Handle_SearchTerm_MatchesDescription()
    {
        var described = CityIntegrationHelpers.BuildCity(DescribedTitle);
        described.Description = DescriptionText;
        await SeedAsync(described, CityIntegrationHelpers.BuildCity(ActiveTitle));

        var (_, value) = await GetAsync($"SearchTerm={DescriptionText.ToLowerInvariant()}");

        Assert.Equal([DescribedTitle], Titles(value));
    }

    [Fact]
    public async Task Handle_Paging_ReturnsRequestedPageWithTotalCount()
    {
        await SeedAsync(Enumerable
            .Range(0, TotalCities)
            .Select(_ => (object)CityIntegrationHelpers.BuildCity())
            .ToArray());

        var (_, value) = await GetAsync($"Page=2&PageSize={PageSize}");

        Assert.Equal(PageSize, Titles(value).Length);
        Assert.Equal(TotalCities, value.GetProperty(TotalCountProperty).GetInt32());
        Assert.Equal(2, value.GetProperty(PageProperty).GetInt32());
        Assert.True(value.GetProperty(HasNextPageProperty).GetBoolean());
    }

    [Fact]
    public async Task Handle_PagingThroughAllPages_ReturnsEveryCityExactlyOnce()
    {
        await SeedAsync(Enumerable
            .Range(0, TotalCities)
            .Select(_ => (object)CityIntegrationHelpers.BuildCity())
            .ToArray());
        var ids = new List<Guid>();

        for (var page = 1; page <= LastPage; page++)
        {
            var (_, value) = await GetAsync($"Page={page}&PageSize={PageSize}");
            ids.AddRange(value
                .GetProperty(ItemsProperty)
                .EnumerateArray()
                .Select(item => item.GetProperty(IdProperty).GetGuid()));
        }

        Assert.Equal(TotalCities, ids.Count);
        Assert.Equal(TotalCities, ids.Distinct().Count());
    }

    [Fact]
    public async Task Handle_Always_ReturnsNewestFirst()
    {

        var oldest = CityIntegrationHelpers.BuildCity();
        var middle = CityIntegrationHelpers.BuildCity();
        var newest = CityIntegrationHelpers.BuildCity();
        await SeedAsync(oldest, middle, newest);
        var now = DateTimeOffset.UtcNow;
        await SetCreatedAtAsync(oldest.Id, now.AddDays(-OldestAgeDays));
        await SetCreatedAtAsync(middle.Id, now.AddDays(-MiddleAgeDays));
        await SetCreatedAtAsync(newest.Id, now.AddDays(-NewestAgeDays));

        var (_, value) = await GetAsync();

        Assert.Equal([newest.Id, middle.Id, oldest.Id], Ids(value));
    }

    [Fact]
    public async Task Handle_ItemsCreatedAtSameMoment_OrdersThemById()
    {

        var first = CityIntegrationHelpers.BuildCity();
        var second = CityIntegrationHelpers.BuildCity();
        var third = CityIntegrationHelpers.BuildCity();
        await SeedAsync(first, second, third);
        var createdAt = DateTimeOffset.UtcNow.AddDays(-NewestAgeDays);
        foreach (var item in new[] { first.Id, second.Id, third.Id })
            await SetCreatedAtAsync(item, createdAt);

        var (_, value) = await GetAsync("IncludeImages=false");

        var expected = new[] { first.Id, second.Id, third.Id }.OrderBy(id => id.ToString(), StringComparer.Ordinal);
        Assert.Equal(expected, Ids(value));
    }

    [Fact]
    public async Task Handle_ImagesIncludedByDefault_ReturnsImageUrls()
    {
        await SeedAsync(CityIntegrationHelpers.BuildCity(
            ActiveTitle,
            CityStatus.Active,
            FirstImageFileName,
            SecondImageFileName));

        var (_, value) = await GetAsync();

        var images = value.GetProperty(ItemsProperty)[0].GetProperty(ImagesProperty);
        Assert.Equal(
            new[]
            {
                EnvironmentConstants.ImageStorage.UrlPrefix + FirstImageFileName,
                EnvironmentConstants.ImageStorage.UrlPrefix + SecondImageFileName
            },
            images.EnumerateArray().Select(image => image.GetProperty(UrlProperty).GetString()).ToArray());
    }

    [Fact]
    public async Task Handle_ImagesExcluded_ReturnsCitiesWithoutImages()
    {
        await SeedAsync(CityIntegrationHelpers.BuildCity(
            ActiveTitle,
            CityStatus.Active,
            FirstImageFileName));

        var (_, value) = await GetAsync("IncludeImages=false");

        Assert.Equal(0, value.GetProperty(ItemsProperty)[0].GetProperty(ImagesProperty).GetArrayLength());
    }

    [Theory]
    [MemberData(nameof(GetCitiesIntegrationTestData.InvalidFilters), MemberType = typeof(GetCitiesIntegrationTestData))]
    public async Task Handle_FilterIsInvalid_ReturnsBadRequest(FilterQueryCase filter)
    {
        var (status, _) = await GetAsync(filter.Query);

        Assert.Equal(HttpStatusCode.BadRequest, status);
    }
}
