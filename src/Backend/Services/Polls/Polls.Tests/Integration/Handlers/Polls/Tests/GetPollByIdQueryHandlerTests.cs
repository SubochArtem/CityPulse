using System.Net;
using System.Text.Json;
using Polls.Domain.Authorization;
using Polls.Domain.Polls.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.Infrastructure.Constants;
using Polls.Tests.Integration.Handlers.Cities;
using Polls.Tests.Integration.Handlers.Polls.TestData;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Polls.Tests;

public sealed class GetPollByIdQueryHandlerTests(PollsWebAppFactory factory) : PollsApiTestBase(factory)
{
    private const string PollTitle = "Park renovation";
    private const string PollDescription = "Where should the money go";
    private const string FirstImageFileName = "first.jpg";
    private const string SecondImageFileName = "second.jpg";
    private const decimal PollBudget = 1500.5m;
    private const string ValueProperty = "value";
    private const string IdProperty = "id";
    private const string TitleProperty = "title";
    private const string DescriptionProperty = "description";
    private const string CityIdProperty = "cityId";
    private const string TypeProperty = "type";
    private const string BudgetAmountProperty = "budgetAmount";
    private const string StatusProperty = "status";
    private const string ImagesProperty = "images";
    private const string UrlProperty = "url";

    private Task<HttpResponseMessage> GetAsAdminAsync(Guid id) =>
        CreateClient([Permissions.Polls.ReadAny])
            .GetAsync($"{PollIntegrationHelpers.AdminPollsRoute}/{id}");

    private Task<HttpResponseMessage> GetAsUserAsync(Guid id) =>
        CreateAnonymousClient()
            .GetAsync($"{PollIntegrationHelpers.UserPollsRoute}/{id}");

    private static async Task<JsonElement> ReadValueAsync(HttpResponseMessage response) =>
        JsonDocument
            .Parse(await response.Content.ReadAsStringAsync())
            .RootElement
            .GetProperty(ValueProperty);

    private async Task<Guid> SeedPollAsync(
        PollStatus status = PollStatus.Active,
        PollType type = PollType.UserVote,
        params string[] imageFileNames)
    {
        var city = CityIntegrationHelpers.BuildCity();
        var poll = PollIntegrationHelpers.BuildPoll(city.Id, status, type, imageFileNames);
        poll.Title = PollTitle;
        poll.Description = PollDescription;
        await SeedAsync(city, poll);

        return poll.Id;
    }

    [Fact]
    public async Task Handle_PollExists_ReturnsItsData()
    {
        var city = CityIntegrationHelpers.BuildCity();
        var poll = PollIntegrationHelpers.BuildPoll(city.Id, PollStatus.Active, PollType.ManagerVote);
        poll.Title = PollTitle;
        poll.Description = PollDescription;
        poll.BudgetAmount = PollBudget;
        await SeedAsync(city, poll);

        var response = await GetAsAdminAsync(poll.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var value = await ReadValueAsync(response);
        Assert.Equal(poll.Id, value.GetProperty(IdProperty).GetGuid());
        Assert.Equal(PollTitle, value.GetProperty(TitleProperty).GetString());
        Assert.Equal(PollDescription, value.GetProperty(DescriptionProperty).GetString());
        Assert.Equal(city.Id, value.GetProperty(CityIdProperty).GetGuid());
        Assert.Equal((int)PollType.ManagerVote, value.GetProperty(TypeProperty).GetInt32());
        Assert.Equal(PollBudget, value.GetProperty(BudgetAmountProperty).GetDecimal());
        Assert.Equal((int)PollStatus.Active, value.GetProperty(StatusProperty).GetInt32());
    }

    [Fact]
    public async Task Handle_PollHasImages_ReturnsImageUrlsInOrder()
    {
        var id = await SeedPollAsync(PollStatus.Active, PollType.UserVote, FirstImageFileName, SecondImageFileName);

        var response = await GetAsAdminAsync(id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var images = (await ReadValueAsync(response)).GetProperty(ImagesProperty);
        Assert.Equal(
            [
                EnvironmentConstants.ImageStorage.UrlPrefix + FirstImageFileName,
                EnvironmentConstants.ImageStorage.UrlPrefix + SecondImageFileName
            ],
            images.EnumerateArray().Select(image => image.GetProperty(UrlProperty).GetString()));
    }

    [Fact]
    public async Task Handle_PollHasNoImages_ReturnsEmptyImageList()
    {
        var id = await SeedPollAsync();

        var value = await ReadValueAsync(await GetAsAdminAsync(id));

        Assert.Equal(0, value.GetProperty(ImagesProperty).GetArrayLength());
    }

    [Theory]
    [MemberData(nameof(GetPollByIdIntegrationTestData.NonActiveStatuses), MemberType = typeof(GetPollByIdIntegrationTestData))]
    public async Task Handle_PollIsNotActive_ReturnsItToAdmin(PollStatus status)
    {
        var id = await SeedPollAsync(status);

        var response = await GetAsAdminAsync(id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal((int)status, (await ReadValueAsync(response)).GetProperty(StatusProperty).GetInt32());
    }

    [Fact]
    public async Task Handle_ActivePoll_ReturnsItToUser()
    {
        var id = await SeedPollAsync();

        var response = await GetAsUserAsync(id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(id, (await ReadValueAsync(response)).GetProperty(IdProperty).GetGuid());
    }

    [Theory]
    [MemberData(nameof(GetPollByIdIntegrationTestData.NonActiveStatuses), MemberType = typeof(GetPollByIdIntegrationTestData))]
    public async Task Handle_PollIsNotActive_ReturnsNotFoundToUser(PollStatus status)
    {
        var id = await SeedPollAsync(status);

        var response = await GetAsUserAsync(id);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Handle_PollDoesNotExist_ReturnsNotFoundOnBothRoutes()
    {
        var id = Guid.NewGuid();

        Assert.Equal(HttpStatusCode.NotFound, (await GetAsAdminAsync(id)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await GetAsUserAsync(id)).StatusCode);
    }
}
