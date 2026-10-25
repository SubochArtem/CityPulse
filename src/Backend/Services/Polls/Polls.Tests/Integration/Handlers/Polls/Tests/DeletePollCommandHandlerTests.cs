using System.Net;
using Microsoft.EntityFrameworkCore;
using Moq;
using Polls.Domain.Authorization;
using Polls.Domain.Cities;
using Polls.Domain.Ideas;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.Integration.Handlers.Cities;
using Polls.Tests.TestData;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Polls.Tests;

public sealed class DeletePollCommandHandlerTests(PollsWebAppFactory factory) : PollsApiTestBase(factory)
{
    private const string FirstImageFileName = "first.jpg";
    private const string SecondImageFileName = "second.jpg";
    private const int ExpiredEditWindowDays = 2;

    private sealed record PollGraph(City City, Poll Poll, Idea Idea);

    private async Task<PollGraph> SeedPollGraphAsync(
        PollStatus status = PollStatus.Active,
        DateTimeOffset? createdAt = null,
        string[]? imageFileNames = null)
    {
        var city = CityIntegrationHelpers.BuildCity();
        var poll = PollIntegrationHelpers.BuildPoll(
            city.Id,
            status,
            PollType.UserVote,
            imageFileNames ?? []);

        var idea = IdeaFakers.Ideas().Generate();
        idea.Id = Guid.NewGuid();
        idea.PollId = poll.Id;

        await SeedAsync(city, poll, idea);

        if (createdAt is not null)
            await ExecuteDbAsync(context => context.Polls
                .Where(x => x.Id == poll.Id)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.CreatedAt, createdAt.Value)));

        return new PollGraph(city, poll, idea);
    }

    private Task<HttpResponseMessage> DeleteAsAdminAsync(Guid id, Guid? callerCityId = null) =>
        CreateClient([Permissions.Polls.DeleteAny], cityId: callerCityId)
            .DeleteAsync($"{PollIntegrationHelpers.AdminPollsRoute}/{id}");

    private Task<HttpResponseMessage> DeleteAsUserAsync(Guid id, Guid userCityId) =>
        CreateClient([Permissions.Polls.DeleteCity], cityId: userCityId)
            .DeleteAsync($"{PollIntegrationHelpers.UserPollsRoute}/{id}");

    private Task<int> CountPollsAsync(Guid id) =>
        QueryDbAsync(context => context.Polls.CountAsync(x => x.Id == id));

    [Fact]
    public async Task Handle_PollExists_RemovesItAndKeepsTheCity()
    {
        var graph = await SeedPollGraphAsync();

        var response = await DeleteAsAdminAsync(graph.Poll.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, await CountPollsAsync(graph.Poll.Id));
        Assert.Equal(1, await QueryDbAsync(context => context.Cities.CountAsync(x => x.Id == graph.City.Id)));
    }

    [Fact]
    public async Task Handle_PollExists_CancelsItsScheduledJobOnce()
    {
        var graph = await SeedPollGraphAsync();

        await DeleteAsAdminAsync(graph.Poll.Id);

        Factory.PollScheduler.Verify(
            x => x.CancelAsync(graph.Poll.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DeleteIsRejected_DoesNotCancelScheduledJob()
    {
        var graph = await SeedPollGraphAsync();

        await DeleteAsUserAsync(graph.Poll.Id, Guid.NewGuid());

        Factory.PollScheduler.Verify(
            x => x.CancelAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_PollHasImages_RemovesThemAndQueuesFilesForDeletion()
    {
        var graph = await SeedPollGraphAsync(imageFileNames: [FirstImageFileName, SecondImageFileName]);

        await DeleteAsAdminAsync(graph.Poll.Id);

        Assert.Equal(0, await QueryDbAsync(context => context.PollImages.CountAsync()));
        var queued = await QueryDbAsync(context => context.DeletedImages.Select(x => x.FileName).ToListAsync());
        Assert.Equivalent(new[] { FirstImageFileName, SecondImageFileName }, queued);
    }

    [Fact]
    public async Task Handle_PollHasIdeas_RemovesThemToo()
    {
        var graph = await SeedPollGraphAsync();

        await DeleteAsAdminAsync(graph.Poll.Id);

        Assert.Equal(0, await QueryDbAsync(context => context.Ideas.CountAsync()));
    }

    [Fact]
    public async Task Handle_Always_DoesNotTouchOtherPolls()
    {
        var target = await SeedPollGraphAsync(imageFileNames: [FirstImageFileName]);
        var other = await SeedPollGraphAsync(imageFileNames: [SecondImageFileName]);

        await DeleteAsAdminAsync(target.Poll.Id);

        Assert.Equal(1, await CountPollsAsync(other.Poll.Id));
        Assert.Equal(1, await QueryDbAsync(context => context.Ideas.CountAsync(x => x.Id == other.Idea.Id)));
        Assert.Equal(1, await QueryDbAsync(context => context.PollImages.CountAsync(x => x.PollId == other.Poll.Id)));
    }

    [Fact]
    public async Task Handle_PollDoesNotExist_ReturnsNotFound()
    {
        var response = await DeleteAsAdminAsync(Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Handle_PollIsDeletedTwice_ReturnsNotFoundTheSecondTime()
    {
        var graph = await SeedPollGraphAsync();
        await DeleteAsAdminAsync(graph.Poll.Id);

        var response = await DeleteAsAdminAsync(graph.Poll.Id);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Handle_UserDeletesPollOfOwnCity_RemovesIt()
    {
        var graph = await SeedPollGraphAsync();

        var response = await DeleteAsUserAsync(graph.Poll.Id, graph.City.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, await CountPollsAsync(graph.Poll.Id));
    }

    [Fact]
    public async Task Handle_UserDeletesPollOfAnotherCity_ReturnsForbiddenAndKeepsPoll()
    {
        var graph = await SeedPollGraphAsync();

        var response = await DeleteAsUserAsync(graph.Poll.Id, Guid.NewGuid());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(1, await CountPollsAsync(graph.Poll.Id));
    }

    [Theory]
    [InlineData(PollStatus.Suspended)]
    [InlineData(PollStatus.Finished)]
    public async Task Handle_UserDeletesPollThatIsNotOpen_ReturnsConflictAndKeepsPoll(PollStatus status)
    {
        var graph = await SeedPollGraphAsync(status);

        var response = await DeleteAsUserAsync(graph.Poll.Id, graph.City.Id);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(1, await CountPollsAsync(graph.Poll.Id));
    }

    [Fact]
    public async Task Handle_UserDeletesPollAfterEditWindow_ReturnsConflictAndKeepsPoll()
    {
        var graph = await SeedPollGraphAsync(createdAt: DateTimeOffset.UtcNow.AddDays(-ExpiredEditWindowDays));

        var response = await DeleteAsUserAsync(graph.Poll.Id, graph.City.Id);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(1, await CountPollsAsync(graph.Poll.Id));
    }

    [Fact]
    public async Task Handle_AdminDeletesRestrictedPoll_BypassesCityStatusAndEditWindowChecks()
    {
        var graph = await SeedPollGraphAsync(
            PollStatus.Finished,
            DateTimeOffset.UtcNow.AddDays(-ExpiredEditWindowDays));

        var response = await DeleteAsAdminAsync(graph.Poll.Id, Guid.NewGuid());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, await CountPollsAsync(graph.Poll.Id));
    }
}
