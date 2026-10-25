using System.Net;
using System.Text.Json;
using Polls.Domain.Authorization;
using Polls.Domain.Cities.Enums;
using Polls.Domain.Images;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.Infrastructure.Constants;
using Polls.Tests.TestData;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Cities.Tests;

public sealed class GetCityWithPollsQueryHandlerTests(PollsWebAppFactory factory) : PollsApiTestBase(factory)
{
    private const string CityTitle = "Springfield";
    private const string CityImageFileName = "city.jpg";
    private const string FirstPollImageFileName = "poll-first.jpg";
    private const string SecondPollImageFileName = "poll-second.jpg";
    private const string PollsSegment = "polls";
    private const string IdProperty = "id";
    private const string TitleProperty = "title";
    private const string StatusProperty = "status";
    private const string PollsProperty = "polls";
    private const string ImagesProperty = "images";
    private const string UrlProperty = "url";

    private async Task<HttpResponseMessage> GetAsync(Guid cityId) =>
        await CreateClient([Permissions.Cities.ReadAny])
            .GetAsync($"{CityIntegrationHelpers.AdminCitiesRoute}/{cityId}/{PollsSegment}");

    private static async Task<JsonElement> ReadValueAsync(HttpResponseMessage response) =>
        JsonDocument
            .Parse(await response.Content.ReadAsStringAsync())
            .RootElement;

    private static Poll BuildPoll(Guid cityId, PollStatus status, params string[] imageFileNames)
    {
        var id = Guid.NewGuid();

        var poll = PollFakers.Polls().Generate();
        poll.Id = id;
        poll.CityId = cityId;
        poll.Status = status;
        poll.Images = imageFileNames
            .Select((fileName, order) => new PollImage
            {
                Id = Guid.NewGuid(),
                FileName = fileName,
                Order = order,
                PollId = id
            })
            .ToList();

        return poll;
    }

    private static Guid[] PollIds(JsonElement value) =>
        value
            .GetProperty(PollsProperty)
            .EnumerateArray()
            .Select(poll => poll.GetProperty(IdProperty).GetGuid())
            .ToArray();

    [Fact]
    public async Task Handle_CityHasPolls_ReturnsCityWithItsPollsOnly()
    {
        var city = CityIntegrationHelpers.BuildCity(CityTitle);
        var otherCity = CityIntegrationHelpers.BuildCity();
        var firstPoll = BuildPoll(city.Id, PollStatus.Active);
        var secondPoll = BuildPoll(city.Id, PollStatus.Active);
        var otherPoll = BuildPoll(otherCity.Id, PollStatus.Active);
        await SeedAsync(city, otherCity, firstPoll, secondPoll, otherPoll);

        var response = await GetAsync(city.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var value = await ReadValueAsync(response);
        Assert.Equal(city.Id, value.GetProperty(IdProperty).GetGuid());
        Assert.Equal(CityTitle, value.GetProperty(TitleProperty).GetString());
        Assert.Equivalent(new[] { firstPoll.Id, secondPoll.Id }, PollIds(value));
    }

    [Fact]
    public async Task Handle_PollsHaveDifferentStatuses_ReturnsAllOfThemToAdmin()
    {
        var city = CityIntegrationHelpers.BuildCity();
        var activePoll = BuildPoll(city.Id, PollStatus.Active);
        var suspendedPoll = BuildPoll(city.Id, PollStatus.Suspended);
        await SeedAsync(city, activePoll, suspendedPoll);

        var value = await ReadValueAsync(await GetAsync(city.Id));

        var statuses = value
            .GetProperty(PollsProperty)
            .EnumerateArray()
            .Select(poll => poll.GetProperty(StatusProperty).GetInt32())
            .ToArray();
        Assert.Equivalent(new[] { (int)PollStatus.Active, (int)PollStatus.Suspended }, statuses);
    }

    [Fact]
    public async Task Handle_CityAndPollsHaveImages_ReturnsImageUrlsFromStorage()
    {
        var city = CityIntegrationHelpers.BuildCity(CityTitle, CityStatus.Active, CityImageFileName);
        var poll = BuildPoll(city.Id, PollStatus.Active, FirstPollImageFileName, SecondPollImageFileName);
        await SeedAsync(city, poll);

        var value = await ReadValueAsync(await GetAsync(city.Id));

        var cityImages = value.GetProperty(ImagesProperty);
        Assert.Equal(
            [EnvironmentConstants.ImageStorage.UrlPrefix + CityImageFileName],
            cityImages.EnumerateArray().Select(image => image.GetProperty(UrlProperty).GetString()));
        var pollImages = value.GetProperty(PollsProperty)[0].GetProperty(ImagesProperty);
        Assert.Equal(
            [
                EnvironmentConstants.ImageStorage.UrlPrefix + FirstPollImageFileName,
                EnvironmentConstants.ImageStorage.UrlPrefix + SecondPollImageFileName
            ],
            pollImages.EnumerateArray().Select(image => image.GetProperty(UrlProperty).GetString()));
    }

    [Fact]
    public async Task Handle_CityHasNoPolls_ReturnsEmptyPollList()
    {
        var city = CityIntegrationHelpers.BuildCity(CityTitle);
        await SeedAsync(city);

        var response = await GetAsync(city.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(PollIds(await ReadValueAsync(response)));
    }

    [Fact]
    public async Task Handle_CityIsInactive_ReturnsItWithPollsToAdmin()
    {
        var city = CityIntegrationHelpers.BuildCity(CityTitle, CityStatus.Inactive);
        var poll = BuildPoll(city.Id, PollStatus.Suspended);
        await SeedAsync(city, poll);

        var response = await GetAsync(city.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal([poll.Id], PollIds(await ReadValueAsync(response)));
    }

    [Fact]
    public async Task Handle_CityDoesNotExist_ReturnsNotFound()
    {
        var response = await GetAsync(Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
