using System.Net;
using System.Net.Http.Json;
using CityPulse.Contracts.Querying.Pagination;
using Users.Business.DTOs;
using Users.DataAccess.Entities;
using Users.DataAccess.Entities.Enums;
using Users.Tests.Infrastructure;
using Users.Tests.Integration.Users.TestData;
using Xunit;

namespace Users.Tests.Integration.Users.Tests;

public sealed class GetUsersTests(IntegrationTestWebAppFactory factory) : UsersApiTestBase(factory)
{
    private const string UsersRoute = "api/v1/Users";
    private const string UnknownNickname = "nobody_like_this";
    private const int PageSizeTwo = 2;
    private const int SeededUsersCount = 5;
    private const int LastPage = 3;
    private const int PageBeyondLast = 4;

    [Fact]
    public async Task GetUsers_NoFilters_ReturnsAllUsers()
    {
        var users = await SeedUsersAsync(GetUsersTestData.SeededNicknames);

        var result = await GetPagedAsync(UsersRoute);

        Assert.Equal(users.Count, result.TotalCount);
        Assert.Equal(users.Select(u => u.Id).Order(), result.Items.Select(u => u.Id).Order());
    }

    [Fact]
    public async Task GetUsers_NoUsers_ReturnsEmptyPage()
    {
        var result = await GetPagedAsync(UsersRoute);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Theory]
    [MemberData(nameof(GetUsersTestData.NicknameSearches), MemberType = typeof(GetUsersTestData))]
    public async Task GetUsers_FilterByNickname_ReturnsMatchingUsersIgnoringCase(string search, string[] expectedNicknames)
    {
        await SeedUsersAsync(GetUsersTestData.SeededNicknames);

        var result = await GetPagedAsync($"{UsersRoute}?Nickname={search}");

        Assert.Equal(expectedNicknames.Length, result.TotalCount);
        Assert.Equal(expectedNicknames.Order(), result.Items.Select(u => u.Nickname).Order());
    }

    [Fact]
    public async Task GetUsers_FilterByNickname_NoMatch_ReturnsEmptyPage()
    {
        await SeedUsersAsync(GetUsersTestData.SeededNicknames);

        var result = await GetPagedAsync($"{UsersRoute}?Nickname={UnknownNickname}");

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task GetUsers_FilterByCityId_ReturnsOnlyUsersFromCity()
    {
        var cityId = Guid.NewGuid();
        var inCity = await SeedUserAsync(CreateUser(GetUsersTestData.AliceSmith, cityId));
        await SeedUserAsync(CreateUser(GetUsersTestData.BobSmith, Guid.NewGuid()));
        await SeedUserAsync(CreateUser(GetUsersTestData.AliceJones, cityId: null));

        var result = await GetPagedAsync($"{UsersRoute}?CityId={cityId}");

        var item = Assert.Single(result.Items);
        Assert.Equal(inCity.Id, item.Id);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task GetUsers_FilterByNicknameAndCityId_AppliesBothFilters()
    {
        var cityId = Guid.NewGuid();
        var expected = await SeedUserAsync(CreateUser(GetUsersTestData.AliceSmith, cityId));
        await SeedUserAsync(CreateUser(GetUsersTestData.AliceJones, Guid.NewGuid()));
        await SeedUserAsync(CreateUser(GetUsersTestData.BobSmith, cityId));

        var result = await GetPagedAsync($"{UsersRoute}?Nickname=alice&CityId={cityId}");

        var item = Assert.Single(result.Items);
        Assert.Equal(expected.Id, item.Id);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 2)]
    [InlineData(LastPage, 1)]
    public async Task GetUsers_Pagination_ReturnsRequestedPage(int page, int expectedItemsCount)
    {
        await SeedUsersAsync(Enumerable.Range(0, SeededUsersCount).Select(i => $"user_{i}"));

        var result = await GetPagedAsync($"{UsersRoute}?Page={page}&PageSize={PageSizeTwo}");

        Assert.Equal(expectedItemsCount, result.Items.Count);
        Assert.Equal(page, result.Page);
        Assert.Equal(PageSizeTwo, result.PageSize);
        Assert.Equal(SeededUsersCount, result.TotalCount);
    }

    [Fact]
    public async Task GetUsers_NoFilters_ReturnsNewestUsersFirst()
    {
        var users = await SeedUsersAsync(GetUsersTestData.SeededNicknames);

        var result = await GetPagedAsync(UsersRoute);

        Assert.Equal(
            users.OrderByDescending(u => u.CreatedAt).Select(u => u.Id),
            result.Items.Select(u => u.Id));
    }

    [Fact]
    public async Task GetUsers_AllPages_ReturnEveryUserExactlyOnceInStableOrder()
    {
        var users = await SeedUsersAsync(Enumerable.Range(0, SeededUsersCount).Select(i => $"user_{i}"));

        var ids = new List<Guid>();
        for (var page = 1; page <= LastPage; page++)
        {
            var result = await GetPagedAsync($"{UsersRoute}?Page={page}&PageSize={PageSizeTwo}");
            ids.AddRange(result.Items.Select(u => u.Id));
        }

        Assert.Equal(users.OrderByDescending(u => u.CreatedAt).Select(u => u.Id), ids);
    }

    [Fact]
    public async Task GetUsers_PageBeyondLastPage_ReturnsEmptyItemsWithTotalCount()
    {
        await SeedUsersAsync(Enumerable.Range(0, SeededUsersCount).Select(i => $"user_{i}"));

        var result = await GetPagedAsync($"{UsersRoute}?Page={PageBeyondLast}&PageSize={PageSizeTwo}");

        Assert.Empty(result.Items);
        Assert.Equal(SeededUsersCount, result.TotalCount);
    }

    private async Task<PagedList<GetUserDto>> GetPagedAsync(string url)
    {
        var response = await Client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedList<GetUserDto>>();
        Assert.NotNull(result);
        return result;
    }

    private async Task<List<User>> SeedUsersAsync(IEnumerable<string> nicknames)
    {
        var users = new List<User>();
        foreach (var nickname in nicknames)
        {
            users.Add(await SeedUserAsync(CreateUser(nickname, Guid.NewGuid())));
        }

        return users;
    }

    private static User CreateUser(string nickname, Guid? cityId) => new()
    {
        Id = Guid.NewGuid(),
        IdentityId = $"auth0|{Guid.NewGuid():N}",
        Nickname = nickname,
        CityId = cityId,
        AccessStatus = UserAccessStatus.Active,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };
}
