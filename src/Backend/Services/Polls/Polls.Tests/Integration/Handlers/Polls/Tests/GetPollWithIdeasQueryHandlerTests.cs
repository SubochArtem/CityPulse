using System.Net;
using System.Text.Json;
using Polls.Domain.Authorization;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Images;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.Infrastructure.Constants;
using Polls.Tests.Integration.Handlers.Cities;
using Polls.Tests.TestData;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Polls.Tests;

public sealed class GetPollWithIdeasQueryHandlerTests(PollsWebAppFactory factory) : PollsApiTestBase(factory)
{
    private const string IdeasSegment = "ideas";
    private const string FirstIdeaTitle = "More benches";
    private const string SecondIdeaTitle = "Playground";
    private const string ThirdIdeaTitle = "Fountain";
    private const string FirstImageFileName = "first.jpg";
    private const string SecondImageFileName = "second.jpg";
    private const string IdProperty = "id";
    private const string IdeasProperty = "ideas";
    private const string TitleProperty = "title";
    private const string UserIdProperty = "userId";
    private const string PollIdProperty = "pollId";
    private const string AccessStatusProperty = "accessStatus";
    private const string ApprovalStatusProperty = "approvalStatus";
    private const string ImagesProperty = "images";
    private const string UrlProperty = "url";

    private Task<HttpResponseMessage> GetAsAdminAsync(Guid pollId) =>
        CreateClient([Permissions.Polls.ReadAny])
            .GetAsync($"{PollIntegrationHelpers.AdminPollsRoute}/{pollId}/{IdeasSegment}");

    private Task<HttpResponseMessage> GetAsUserAsync(Guid pollId) =>
        CreateAnonymousClient()
            .GetAsync($"{PollIntegrationHelpers.UserPollsRoute}/{pollId}/{IdeasSegment}");

    private static async Task<JsonElement> ReadValueAsync(HttpResponseMessage response) =>
        JsonDocument
            .Parse(await response.Content.ReadAsStringAsync())
            .RootElement;

    private async Task<Poll> SeedPollAsync(PollStatus status = PollStatus.Active)
    {
        var city = CityIntegrationHelpers.BuildCity();
        var poll = PollIntegrationHelpers.BuildPoll(city.Id, status);
        await SeedAsync(city, poll);

        return poll;
    }

    private static Idea BuildIdea(
        Guid pollId,
        string title,
        IdeaAccessStatus accessStatus = IdeaAccessStatus.Active,
        params string[] imageFileNames)
    {
        var id = Guid.NewGuid();

        var idea = IdeaFakers.Ideas().Generate();
        idea.Id = id;
        idea.PollId = pollId;
        idea.Title = title;
        idea.AccessStatus = accessStatus;
        idea.Images = imageFileNames
            .Select((fileName, order) => new IdeaImage
            {
                Id = Guid.NewGuid(),
                FileName = fileName,
                Order = order,
                IdeaId = id
            })
            .ToList();

        return idea;
    }

    private static string[] IdeaTitles(JsonElement value) =>
        value
            .GetProperty(IdeasProperty)
            .EnumerateArray()
            .Select(idea => idea.GetProperty(TitleProperty).GetString()!)
            .ToArray();

    [Fact]
    public async Task Handle_PollHasIdeas_ReturnsPollWithItsIdeasOnly()
    {
        var poll = await SeedPollAsync();
        var otherPoll = await SeedPollAsync();
        await SeedAsync(
            BuildIdea(poll.Id, FirstIdeaTitle),
            BuildIdea(poll.Id, SecondIdeaTitle),
            BuildIdea(otherPoll.Id, ThirdIdeaTitle));

        var response = await GetAsAdminAsync(poll.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var value = await ReadValueAsync(response);
        Assert.Equal(poll.Id, value.GetProperty(IdProperty).GetGuid());
        Assert.Equivalent(new[] { FirstIdeaTitle, SecondIdeaTitle }, IdeaTitles(value));
    }

    [Fact]
    public async Task Handle_IdeaExists_ReturnsItsDataAndImageUrls()
    {
        var poll = await SeedPollAsync();
        var idea = BuildIdea(
            poll.Id,
            FirstIdeaTitle,
            IdeaAccessStatus.Active,
            FirstImageFileName,
            SecondImageFileName);
        await SeedAsync(idea);

        var value = await ReadValueAsync(await GetAsAdminAsync(poll.Id));

        var returned = value.GetProperty(IdeasProperty)[0];
        Assert.Equal(idea.Id, returned.GetProperty(IdProperty).GetGuid());
        Assert.Equal(idea.UserId, returned.GetProperty(UserIdProperty).GetGuid());
        Assert.Equal(poll.Id, returned.GetProperty(PollIdProperty).GetGuid());
        Assert.Equal((int)idea.AccessStatus, returned.GetProperty(AccessStatusProperty).GetInt32());
        Assert.Equal((int)idea.ApprovalStatus, returned.GetProperty(ApprovalStatusProperty).GetInt32());
        Assert.Equal(
            [
                EnvironmentConstants.ImageStorage.UrlPrefix + FirstImageFileName,
                EnvironmentConstants.ImageStorage.UrlPrefix + SecondImageFileName
            ],
            returned
                .GetProperty(ImagesProperty)
                .EnumerateArray()
                .Select(image => image.GetProperty(UrlProperty).GetString()));
    }

    [Fact]
    public async Task Handle_AdminRequest_ReturnsIdeasOfAllAccessStatuses()
    {
        var poll = await SeedPollAsync();
        await SeedAsync(
            BuildIdea(poll.Id, FirstIdeaTitle, IdeaAccessStatus.Active),
            BuildIdea(poll.Id, SecondIdeaTitle, IdeaAccessStatus.RestrictedByContext),
            BuildIdea(poll.Id, ThirdIdeaTitle, IdeaAccessStatus.RestrictedByAuthor));

        var value = await ReadValueAsync(await GetAsAdminAsync(poll.Id));

        Assert.Equivalent(new[] { FirstIdeaTitle, SecondIdeaTitle, ThirdIdeaTitle }, IdeaTitles(value));
    }

    [Fact]
    public async Task Handle_UserRequest_ReturnsOnlyIdeasWithActiveAccess()
    {
        var poll = await SeedPollAsync();
        await SeedAsync(
            BuildIdea(poll.Id, FirstIdeaTitle, IdeaAccessStatus.Active),
            BuildIdea(poll.Id, SecondIdeaTitle, IdeaAccessStatus.RestrictedByContext),
            BuildIdea(poll.Id, ThirdIdeaTitle, IdeaAccessStatus.RestrictedByAuthor));

        var value = await ReadValueAsync(await GetAsUserAsync(poll.Id));

        Assert.Equal([FirstIdeaTitle], IdeaTitles(value));
    }

    [Fact]
    public async Task Handle_PollHasNoIdeas_ReturnsEmptyIdeaList()
    {
        var poll = await SeedPollAsync();

        var response = await GetAsAdminAsync(poll.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(IdeaTitles(await ReadValueAsync(response)));
    }

    [Fact]
    public async Task Handle_PollIsNotActive_ReturnsItToAdminButNotFoundToUser()
    {
        var poll = await SeedPollAsync(PollStatus.Suspended);

        Assert.Equal(HttpStatusCode.OK, (await GetAsAdminAsync(poll.Id)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await GetAsUserAsync(poll.Id)).StatusCode);
    }

    [Fact]
    public async Task Handle_PollDoesNotExist_ReturnsNotFoundOnBothRoutes()
    {
        var id = Guid.NewGuid();

        Assert.Equal(HttpStatusCode.NotFound, (await GetAsAdminAsync(id)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await GetAsUserAsync(id)).StatusCode);
    }
}
