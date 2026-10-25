using System.Net;
using System.Text.Json;
using Polls.Domain.Authorization;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Polls.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.Infrastructure.Constants;
using Polls.Tests.Integration.Handlers.Cities;
using Polls.Tests.Integration.Handlers.Ideas.TestData;
using Polls.Tests.Integration.Handlers.Polls;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Ideas.Tests;

public sealed class GetIdeaWithPollQueryHandlerTests(PollsWebAppFactory factory) : PollsApiTestBase(factory)
{
    private const string PollImageFileName = "poll.jpg";
    private const string IdProperty = "id";
    private const string PollProperty = "poll";
    private const string TitleProperty = "title";
    private const string CityIdProperty = "cityId";
    private const string StatusProperty = "status";
    private const string ImagesProperty = "images";
    private const string UrlProperty = "url";

    private Task<HttpResponseMessage> GetAsAdminAsync(Guid id) =>
        CreateClient([Permissions.Ideas.ReadAny])
            .GetAsync($"{IdeaIntegrationHelpers.AdminIdeasRoute}/{id}/{IdeaIntegrationHelpers.PollSegment}");

    private Task<HttpResponseMessage> GetAsUserAsync(Guid id) =>
        CreateAnonymousClient()
            .GetAsync($"{IdeaIntegrationHelpers.UserIdeasRoute}/{id}/{IdeaIntegrationHelpers.PollSegment}");

    private static async Task<JsonElement> ReadValueAsync(HttpResponseMessage response) =>
        JsonDocument
            .Parse(await response.Content.ReadAsStringAsync())
            .RootElement;

    [Fact]
    public async Task Handle_IdeaExists_ReturnsItWithItsPoll()
    {
        var graph = IdeaIntegrationHelpers.BuildGraph();
        await SeedAsync(graph.Entities);

        var response = await GetAsAdminAsync(graph.Idea.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var value = await ReadValueAsync(response);
        Assert.Equal(graph.Idea.Id, value.GetProperty(IdProperty).GetGuid());
        var poll = value.GetProperty(PollProperty);
        Assert.Equal(graph.Poll.Id, poll.GetProperty(IdProperty).GetGuid());
        Assert.Equal(graph.Poll.Title, poll.GetProperty(TitleProperty).GetString());
        Assert.Equal(graph.City.Id, poll.GetProperty(CityIdProperty).GetGuid());
    }

    [Fact]
    public async Task Handle_PollHasImages_ReturnsTheirUrlsFromStorage()
    {
        var city = CityIntegrationHelpers.BuildCity();
        var poll = PollIntegrationHelpers.BuildPoll(
            city.Id,
            PollStatus.Active,
            PollType.UserVote,
            PollImageFileName);
        var idea = IdeaIntegrationHelpers.BuildIdea(poll.Id);
        await SeedAsync(city, poll, idea);

        var value = await ReadValueAsync(await GetAsAdminAsync(idea.Id));

        var images = value.GetProperty(PollProperty).GetProperty(ImagesProperty);
        Assert.Equal(
            [EnvironmentConstants.ImageStorage.UrlPrefix + PollImageFileName],
            images.EnumerateArray().Select(image => image.GetProperty(UrlProperty).GetString()));
    }

    [Fact]
    public async Task Handle_PollIsNotActive_StillReturnsIdeaWithPollToUser()
    {
        var graph = IdeaIntegrationHelpers.BuildGraph(pollStatus: PollStatus.Finished);
        await SeedAsync(graph.Entities);

        var response = await GetAsUserAsync(graph.Idea.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var poll = (await ReadValueAsync(response)).GetProperty(PollProperty);
        Assert.Equal((int)PollStatus.Finished, poll.GetProperty(StatusProperty).GetInt32());
    }

    [Theory]
    [MemberData(nameof(GetIdeaIntegrationTestData.RestrictedAccessStatuses), MemberType = typeof(GetIdeaIntegrationTestData))]
    public async Task Handle_IdeaIsRestricted_ReturnsItToAdminButNotFoundToUser(IdeaAccessStatus status)
    {
        var graph = IdeaIntegrationHelpers.BuildGraph(accessStatus: status);
        await SeedAsync(graph.Entities);

        Assert.Equal(HttpStatusCode.OK, (await GetAsAdminAsync(graph.Idea.Id)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await GetAsUserAsync(graph.Idea.Id)).StatusCode);
    }

    [Fact]
    public async Task Handle_IdeaDoesNotExist_ReturnsNotFoundOnBothRoutes()
    {
        var id = Guid.NewGuid();

        Assert.Equal(HttpStatusCode.NotFound, (await GetAsAdminAsync(id)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await GetAsUserAsync(id)).StatusCode);
    }
}
