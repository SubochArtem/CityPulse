using System.Net;
using Microsoft.EntityFrameworkCore;
using Moq;
using Polls.Domain.Authorization;
using Polls.Domain.Cities;
using Polls.Domain.Cities.Enums;
using Polls.Domain.Ideas;
using Polls.Domain.Polls;
using Polls.Tests.Infrastructure;
using Polls.Tests.Integration.Handlers.Polls;
using Polls.Tests.TestData;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Cities.Tests;

public sealed class DeleteCityCommandHandlerTests(PollsWebAppFactory factory) : PollsApiTestBase(factory)
{
    private const string FirstImageFileName = "first.jpg";
    private const string SecondImageFileName = "second.jpg";

    private sealed record CityGraph(City City, Poll Poll, Idea Idea);

    private Task<HttpResponseMessage> DeleteAsync(Guid id) =>
        CreateClient([Permissions.Cities.DeleteAny])
            .DeleteAsync($"{CityIntegrationHelpers.AdminCitiesRoute}/{id}");

    private async Task<CityGraph> SeedCityGraphAsync(params string[] imageFileNames)
    {
        var city = CityIntegrationHelpers.BuildCity(null, CityStatus.Active, imageFileNames);

        var poll = PollFakers.Polls().Generate();
        poll.Id = Guid.NewGuid();
        poll.CityId = city.Id;

        var idea = IdeaFakers.Ideas().Generate();
        idea.Id = Guid.NewGuid();
        idea.PollId = poll.Id;

        await SeedAsync(city, poll, idea);

        return new CityGraph(city, poll, idea);
    }

    [Fact]
    public async Task Handle_CityExists_RemovesIt()
    {
        var graph = await SeedCityGraphAsync();

        var response = await DeleteAsync(graph.City.Id);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(0, await QueryDbAsync(context => context.Cities.CountAsync()));
    }

    [Fact]
    public async Task Handle_CityHasImages_RemovesThemAndQueuesFilesForDeletion()
    {
        var graph = await SeedCityGraphAsync(FirstImageFileName, SecondImageFileName);

        await DeleteAsync(graph.City.Id);

        Assert.Equal(0, await QueryDbAsync(context => context.CityImages.CountAsync()));
        var queued = await QueryDbAsync(context => context.DeletedImages.Select(x => x.FileName).ToListAsync());
        Assert.Equivalent(new[] { FirstImageFileName, SecondImageFileName }, queued);
    }

    [Fact]
    public async Task Handle_CityHasPollsAndIdeas_RemovesThemToo()
    {
        var graph = await SeedCityGraphAsync();

        await DeleteAsync(graph.City.Id);

        Assert.Equal(0, await QueryDbAsync(context => context.Polls.CountAsync()));
        Assert.Equal(0, await QueryDbAsync(context => context.Ideas.CountAsync()));
    }

    [Fact]
    public async Task Handle_Always_DoesNotTouchOtherCities()
    {
        var target = await SeedCityGraphAsync(FirstImageFileName);
        var other = await SeedCityGraphAsync(SecondImageFileName);

        await DeleteAsync(target.City.Id);

        Assert.Equal(1, await QueryDbAsync(context => context.Cities.CountAsync(x => x.Id == other.City.Id)));
        Assert.Equal(1, await QueryDbAsync(context => context.Polls.CountAsync(x => x.Id == other.Poll.Id)));
        Assert.Equal(1, await QueryDbAsync(context => context.Ideas.CountAsync(x => x.Id == other.Idea.Id)));
        Assert.Equal(1, await QueryDbAsync(context => context.CityImages.CountAsync(x => x.CityId == other.City.Id)));
    }

    [Fact]
    public async Task Handle_CityHasPolls_CancelsTheirScheduledJobsInOneBatch()
    {
        var target = await SeedCityGraphAsync();
        var secondPoll = PollIntegrationHelpers.BuildPoll(target.City.Id);
        await SeedCityGraphAsync();
        await SeedAsync(secondPoll);
        IReadOnlyCollection<Guid>? cancelled = null;
        Factory.PollScheduler
            .Setup(x => x.CancelRangeAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<Guid>, CancellationToken>((ids, _) => cancelled = ids.ToList())
            .Returns(Task.CompletedTask);

        await DeleteAsync(target.City.Id);

        Assert.Equivalent(new[] { target.Poll.Id, secondPoll.Id }, cancelled);
        Factory.PollScheduler.Verify(
            x => x.CancelRangeAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CityHasNoPolls_DoesNotCancelAnything()
    {
        var city = CityIntegrationHelpers.BuildCity();
        await SeedAsync(city);

        var response = await DeleteAsync(city.Id);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Factory.PollScheduler.Verify(
            x => x.CancelRangeAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_CityDoesNotExist_ReturnsNotFoundAndCancelsNothing()
    {
        var response = await DeleteAsync(Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Factory.PollScheduler.Verify(
            x => x.CancelRangeAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_CityIsDeletedTwice_ReturnsNotFoundTheSecondTime()
    {
        var graph = await SeedCityGraphAsync();
        await DeleteAsync(graph.City.Id);

        var response = await DeleteAsync(graph.City.Id);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
