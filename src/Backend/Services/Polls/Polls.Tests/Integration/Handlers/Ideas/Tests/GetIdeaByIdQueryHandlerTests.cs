using System.Net;
using System.Text.Json;
using Polls.Domain.Authorization;
using Polls.Domain.Ideas.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.Infrastructure.Constants;
using Polls.Tests.Integration.Handlers.Ideas.TestData;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Ideas.Tests;

public sealed class GetIdeaByIdQueryHandlerTests(PollsWebAppFactory factory) : PollsApiTestBase(factory)
{
    private const string FirstImageFileName = "first.jpg";
    private const string SecondImageFileName = "second.jpg";
    private const string IdProperty = "id";
    private const string UserIdProperty = "userId";
    private const string PollIdProperty = "pollId";
    private const string TitleProperty = "title";
    private const string DescriptionProperty = "description";
    private const string AccessStatusProperty = "accessStatus";
    private const string ApprovalStatusProperty = "approvalStatus";
    private const string ImagesProperty = "images";
    private const string UrlProperty = "url";

    private Task<HttpResponseMessage> GetAsAdminAsync(Guid id) =>
        CreateClient([Permissions.Ideas.ReadAny])
            .GetAsync($"{IdeaIntegrationHelpers.AdminIdeasRoute}/{id}");

    private Task<HttpResponseMessage> GetAsUserAsync(Guid id) =>
        CreateAnonymousClient()
            .GetAsync($"{IdeaIntegrationHelpers.UserIdeasRoute}/{id}");

    private static async Task<JsonElement> ReadValueAsync(HttpResponseMessage response) =>
        JsonDocument
            .Parse(await response.Content.ReadAsStringAsync())
            .RootElement;

    [Fact]
    public async Task Handle_IdeaExists_ReturnsItsData()
    {
        var graph = IdeaIntegrationHelpers.BuildGraph(
            approvalStatus: IdeaApprovalStatus.Approved);
        graph.Idea.Description = "Along the river";
        await SeedAsync(graph.Entities);

        var response = await GetAsAdminAsync(graph.Idea.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var value = await ReadValueAsync(response);
        Assert.Equal(graph.Idea.Id, value.GetProperty(IdProperty).GetGuid());
        Assert.Equal(graph.Idea.UserId, value.GetProperty(UserIdProperty).GetGuid());
        Assert.Equal(graph.Poll.Id, value.GetProperty(PollIdProperty).GetGuid());
        Assert.Equal(graph.Idea.Title, value.GetProperty(TitleProperty).GetString());
        Assert.Equal(graph.Idea.Description, value.GetProperty(DescriptionProperty).GetString());
        Assert.Equal((int)IdeaAccessStatus.Active, value.GetProperty(AccessStatusProperty).GetInt32());
        Assert.Equal((int)IdeaApprovalStatus.Approved, value.GetProperty(ApprovalStatusProperty).GetInt32());
    }

    [Fact]
    public async Task Handle_IdeaHasImages_ReturnsImageUrlsInOrder()
    {
        var graph = IdeaIntegrationHelpers.BuildGraph(
            imageFileNames: [FirstImageFileName, SecondImageFileName]);
        await SeedAsync(graph.Entities);

        var response = await GetAsAdminAsync(graph.Idea.Id);

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
    public async Task Handle_IdeaHasNoImages_ReturnsEmptyImageList()
    {
        var graph = IdeaIntegrationHelpers.BuildGraph();
        await SeedAsync(graph.Entities);

        var value = await ReadValueAsync(await GetAsAdminAsync(graph.Idea.Id));

        Assert.Equal(0, value.GetProperty(ImagesProperty).GetArrayLength());
    }

    [Fact]
    public async Task Handle_ActiveIdea_ReturnsItToUser()
    {
        var graph = IdeaIntegrationHelpers.BuildGraph();
        await SeedAsync(graph.Entities);

        var response = await GetAsUserAsync(graph.Idea.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(graph.Idea.Id, (await ReadValueAsync(response)).GetProperty(IdProperty).GetGuid());
    }

    [Theory]
    [MemberData(nameof(GetIdeaIntegrationTestData.RestrictedAccessStatuses), MemberType = typeof(GetIdeaIntegrationTestData))]
    public async Task Handle_IdeaIsRestricted_ReturnsItToAdmin(IdeaAccessStatus status)
    {
        var graph = IdeaIntegrationHelpers.BuildGraph(accessStatus: status);
        await SeedAsync(graph.Entities);

        var response = await GetAsAdminAsync(graph.Idea.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal((int)status, (await ReadValueAsync(response)).GetProperty(AccessStatusProperty).GetInt32());
    }

    [Theory]
    [MemberData(nameof(GetIdeaIntegrationTestData.RestrictedAccessStatuses), MemberType = typeof(GetIdeaIntegrationTestData))]
    public async Task Handle_IdeaIsRestricted_ReturnsNotFoundToUser(IdeaAccessStatus status)
    {
        var graph = IdeaIntegrationHelpers.BuildGraph(accessStatus: status);
        await SeedAsync(graph.Entities);

        var response = await GetAsUserAsync(graph.Idea.Id);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Handle_IdeaDoesNotExist_ReturnsNotFoundOnBothRoutes()
    {
        var id = Guid.NewGuid();

        Assert.Equal(HttpStatusCode.NotFound, (await GetAsAdminAsync(id)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await GetAsUserAsync(id)).StatusCode);
    }
}
