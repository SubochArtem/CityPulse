using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Polls.Domain.Authorization;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.Integration.Handlers.Polls.TestData;
using Polls.Tests.TestData;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Polls.Tests;

public sealed class ChangePollStatusCommandHandlerTests(PollsWebAppFactory factory) : PollsApiTestBase(factory)
{
    private const string AdminPollsRoute = "api/v1/admin/polls";
    private const string StatusSegment = "status";
    private const int UnsupportedStatus = 99;

    private sealed record PollGraph(Poll Poll, Idea Idea);

    private async Task<HttpResponseMessage> ChangeStatusAsync(Guid pollId, int newStatus)
    {
        var client = CreateClient([Permissions.Polls.ChangeStatusAny]);

        return await client.PatchAsJsonAsync(
            $"{AdminPollsRoute}/{pollId}/{StatusSegment}",
            new { newStatus });
    }

    private async Task<PollGraph> SeedPollGraphAsync(PollStatus pollStatus, IdeaAccessStatus ideaStatus)
    {
        var city = CityFakers.Cities().Generate();
        city.Id = Guid.NewGuid();
        city.Title = city.Id.ToString();

        var poll = PollFakers.Polls().Generate();
        poll.Id = Guid.NewGuid();
        poll.CityId = city.Id;
        poll.Status = pollStatus;

        var idea = IdeaFakers.Ideas().Generate();
        idea.Id = Guid.NewGuid();
        idea.PollId = poll.Id;
        idea.AccessStatus = ideaStatus;

        await SeedAsync(city, poll, idea);

        return new PollGraph(poll, idea);
    }

    private Task<PollStatus> GetPollStatusAsync(Guid id) =>
        QueryDbAsync(context => context.Polls.Where(x => x.Id == id).Select(x => x.Status).SingleAsync());

    private Task<IdeaAccessStatus> GetIdeaStatusAsync(Guid id) =>
        QueryDbAsync(context => context.Ideas.Where(x => x.Id == id).Select(x => x.AccessStatus).SingleAsync());

    [Fact]
    public async Task ChangeStatus_ActivePollToSuspended_RestrictsIdeas()
    {
        var graph = await SeedPollGraphAsync(PollStatus.Active, IdeaAccessStatus.Active);

        var response = await ChangeStatusAsync(graph.Poll.Id, (int)PollStatus.Suspended);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(PollStatus.Suspended, await GetPollStatusAsync(graph.Poll.Id));
        Assert.Equal(IdeaAccessStatus.RestrictedByContext, await GetIdeaStatusAsync(graph.Idea.Id));
    }

    [Fact]
    public async Task ChangeStatus_SuspendedPollToActive_ReactivatesIdeas()
    {
        var graph = await SeedPollGraphAsync(PollStatus.Suspended, IdeaAccessStatus.RestrictedByContext);

        var response = await ChangeStatusAsync(graph.Poll.Id, (int)PollStatus.Active);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(PollStatus.Active, await GetPollStatusAsync(graph.Poll.Id));
        Assert.Equal(IdeaAccessStatus.Active, await GetIdeaStatusAsync(graph.Idea.Id));
    }

    [Fact]
    public async Task ChangeStatus_Always_DoesNotTouchOtherPolls()
    {
        var target = await SeedPollGraphAsync(PollStatus.Active, IdeaAccessStatus.Active);
        var other = await SeedPollGraphAsync(PollStatus.Active, IdeaAccessStatus.Active);

        await ChangeStatusAsync(target.Poll.Id, (int)PollStatus.Suspended);

        Assert.Equal(PollStatus.Active, await GetPollStatusAsync(other.Poll.Id));
        Assert.Equal(IdeaAccessStatus.Active, await GetIdeaStatusAsync(other.Idea.Id));
    }

    [Fact]
    public async Task ChangeStatus_StatusIsAlreadySet_ReturnsOkAndChangesNothing()
    {
        var graph = await SeedPollGraphAsync(PollStatus.Active, IdeaAccessStatus.Active);

        var response = await ChangeStatusAsync(graph.Poll.Id, (int)PollStatus.Active);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(IdeaAccessStatus.Active, await GetIdeaStatusAsync(graph.Idea.Id));
    }

    [Fact]
    public async Task ChangeStatus_StatusOutsideOfEnum_ReturnsBadRequestAndChangesNothing()
    {
        var graph = await SeedPollGraphAsync(PollStatus.Active, IdeaAccessStatus.Active);

        var response = await ChangeStatusAsync(graph.Poll.Id, UnsupportedStatus);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(PollStatus.Active, await GetPollStatusAsync(graph.Poll.Id));
        Assert.Equal(IdeaAccessStatus.Active, await GetIdeaStatusAsync(graph.Idea.Id));
    }

    [Theory]
    [MemberData(
        nameof(ChangePollStatusIntegrationTestData.NotSupportedStatuses),
        MemberType = typeof(ChangePollStatusIntegrationTestData))]
    public async Task ChangeStatus_StatusIsNotSupported_ReturnsConflictAndChangesNothing(PollStatus status)
    {
        var graph = await SeedPollGraphAsync(PollStatus.Active, IdeaAccessStatus.Active);

        var response = await ChangeStatusAsync(graph.Poll.Id, (int)status);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(PollStatus.Active, await GetPollStatusAsync(graph.Poll.Id));
        Assert.Equal(IdeaAccessStatus.Active, await GetIdeaStatusAsync(graph.Idea.Id));
    }

    [Fact]
    public async Task ChangeStatus_PollDoesNotExist_ReturnsNotFound()
    {
        var response = await ChangeStatusAsync(Guid.NewGuid(), (int)PollStatus.Suspended);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
