using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Polls.Domain.Authorization;
using Polls.Domain.Cities;
using Polls.Domain.Cities.Enums;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.TestData;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Cities.Tests;

public sealed class ChangeCityStatusCommandHandlerTests(PollsWebAppFactory factory) : PollsApiTestBase(factory)
{
    private const string AdminCitiesRoute = "api/v1/admin/cities";
    private const string StatusSegment = "status";
    private const int UnsupportedStatus = 99;

    private sealed record CityGraph(City City, Poll Poll, Idea Idea);

    private async Task<HttpResponseMessage> ChangeStatusAsync(Guid cityId, int newStatus)
    {
        var client = CreateClient([Permissions.Cities.ChangeStatusAny]);

        return await client.PatchAsJsonAsync(
            $"{AdminCitiesRoute}/{cityId}/{StatusSegment}",
            new { newStatus });
    }

    private async Task<CityGraph> SeedCityGraphAsync(
        CityStatus cityStatus,
        PollStatus pollStatus,
        IdeaAccessStatus ideaStatus)
    {
        var city = CityFakers.Cities().Generate();
        city.Id = Guid.NewGuid();
        city.Title = city.Id.ToString();
        city.Status = cityStatus;

        var poll = PollFakers.Polls().Generate();
        poll.Id = Guid.NewGuid();
        poll.CityId = city.Id;
        poll.Status = pollStatus;

        var idea = IdeaFakers.Ideas().Generate();
        idea.Id = Guid.NewGuid();
        idea.PollId = poll.Id;
        idea.AccessStatus = ideaStatus;

        await SeedAsync(city, poll, idea);

        return new CityGraph(city, poll, idea);
    }

    private Task<CityStatus> GetCityStatusAsync(Guid id) =>
        QueryDbAsync(context => context.Cities.Where(x => x.Id == id).Select(x => x.Status).SingleAsync());

    private Task<PollStatus> GetPollStatusAsync(Guid id) =>
        QueryDbAsync(context => context.Polls.Where(x => x.Id == id).Select(x => x.Status).SingleAsync());

    private Task<IdeaAccessStatus> GetIdeaStatusAsync(Guid id) =>
        QueryDbAsync(context => context.Ideas.Where(x => x.Id == id).Select(x => x.AccessStatus).SingleAsync());

    [Fact]
    public async Task ChangeStatus_ActiveCityToInactive_SuspendsPollsAndRestrictsIdeas()
    {
        var graph = await SeedCityGraphAsync(CityStatus.Active, PollStatus.Active, IdeaAccessStatus.Active);

        var response = await ChangeStatusAsync(graph.City.Id, (int)CityStatus.Inactive);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(CityStatus.Inactive, await GetCityStatusAsync(graph.City.Id));
        Assert.Equal(PollStatus.Suspended, await GetPollStatusAsync(graph.Poll.Id));
        Assert.Equal(IdeaAccessStatus.RestrictedByContext, await GetIdeaStatusAsync(graph.Idea.Id));
    }

    [Fact]
    public async Task ChangeStatus_InactiveCityToActive_ReactivatesPollsAndIdeas()
    {
        var graph = await SeedCityGraphAsync(
            CityStatus.Inactive,
            PollStatus.Suspended,
            IdeaAccessStatus.RestrictedByContext);

        var response = await ChangeStatusAsync(graph.City.Id, (int)CityStatus.Active);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(CityStatus.Active, await GetCityStatusAsync(graph.City.Id));
        Assert.Equal(PollStatus.Active, await GetPollStatusAsync(graph.Poll.Id));
        Assert.Equal(IdeaAccessStatus.Active, await GetIdeaStatusAsync(graph.Idea.Id));
    }

    [Fact]
    public async Task ChangeStatus_Always_DoesNotTouchOtherCities()
    {
        var target = await SeedCityGraphAsync(CityStatus.Active, PollStatus.Active, IdeaAccessStatus.Active);
        var other = await SeedCityGraphAsync(CityStatus.Active, PollStatus.Active, IdeaAccessStatus.Active);

        await ChangeStatusAsync(target.City.Id, (int)CityStatus.Inactive);

        Assert.Equal(CityStatus.Active, await GetCityStatusAsync(other.City.Id));
        Assert.Equal(PollStatus.Active, await GetPollStatusAsync(other.Poll.Id));
        Assert.Equal(IdeaAccessStatus.Active, await GetIdeaStatusAsync(other.Idea.Id));
    }

    [Fact]
    public async Task ChangeStatus_CityHasFinishedPoll_KeepsItsStatus()
    {
        var graph = await SeedCityGraphAsync(CityStatus.Active, PollStatus.Finished, IdeaAccessStatus.Active);

        await ChangeStatusAsync(graph.City.Id, (int)CityStatus.Inactive);

        Assert.Equal(PollStatus.Finished, await GetPollStatusAsync(graph.Poll.Id));
    }

    [Fact]
    public async Task ChangeStatus_StatusIsAlreadySet_ReturnsOkAndChangesNothing()
    {
        var graph = await SeedCityGraphAsync(CityStatus.Active, PollStatus.Active, IdeaAccessStatus.Active);

        var response = await ChangeStatusAsync(graph.City.Id, (int)CityStatus.Active);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(PollStatus.Active, await GetPollStatusAsync(graph.Poll.Id));
        Assert.Equal(IdeaAccessStatus.Active, await GetIdeaStatusAsync(graph.Idea.Id));
    }

    [Fact]
    public async Task ChangeStatus_UnsupportedStatus_ReturnsBadRequestAndChangesNothing()
    {
        var graph = await SeedCityGraphAsync(CityStatus.Active, PollStatus.Active, IdeaAccessStatus.Active);

        var response = await ChangeStatusAsync(graph.City.Id, UnsupportedStatus);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(CityStatus.Active, await GetCityStatusAsync(graph.City.Id));
    }

    [Fact]
    public async Task ChangeStatus_CityDoesNotExist_ReturnsNotFound()
    {
        var response = await ChangeStatusAsync(Guid.NewGuid(), (int)CityStatus.Inactive);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
