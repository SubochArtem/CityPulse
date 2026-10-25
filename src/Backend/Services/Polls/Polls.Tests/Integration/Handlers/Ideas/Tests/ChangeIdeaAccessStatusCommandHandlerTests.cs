using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Polls.Domain.Authorization;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Polls.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.Integration.Handlers.Ideas.TestData;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Ideas.Tests;

public sealed class ChangeIdeaAccessStatusCommandHandlerTests(PollsWebAppFactory factory)
    : PollsApiTestBase(factory)
{
    private const int UnsupportedStatus = 99;

    private async Task<HttpResponseMessage> ChangeStatusAsync(Guid id, int newIdeaAccessStatus)
    {
        var client = CreateClient([Permissions.Ideas.ChangeAccessStatusAny]);

        return await client.PatchAsJsonAsync(
            $"{IdeaIntegrationHelpers.AdminIdeasRoute}/{id}/{IdeaIntegrationHelpers.AccessStatusSegment}",
            new { newIdeaAccessStatus });
    }

    private Task<Idea> LoadIdeaAsync(Guid id) =>
        QueryDbAsync(context => context.Ideas.SingleAsync(x => x.Id == id));

    [Theory]
    [MemberData(nameof(ChangeIdeaStatusIntegrationTestData.AccessTransitions), MemberType = typeof(ChangeIdeaStatusIntegrationTestData))]
    public async Task ChangeStatus_ValidTransition_StoresNewStatusAndKeepsApprovalStatus(
        IdeaAccessStatus current,
        IdeaAccessStatus target)
    {
        var graph = IdeaIntegrationHelpers.BuildGraph(
            accessStatus: current,
            approvalStatus: IdeaApprovalStatus.Rejected);
        await SeedAsync(graph.Entities);

        var response = await ChangeStatusAsync(graph.Idea.Id, (int)target);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var stored = await LoadIdeaAsync(graph.Idea.Id);
        Assert.Equal(target, stored.AccessStatus);
        Assert.Equal(IdeaApprovalStatus.Rejected, stored.ApprovalStatus);
    }

    [Fact]
    public async Task ChangeStatus_StatusIsAlreadySet_ReturnsOkAndChangesNothing()
    {
        var graph = IdeaIntegrationHelpers.BuildGraph(accessStatus: IdeaAccessStatus.RestrictedByAuthor);
        await SeedAsync(graph.Entities);

        var response = await ChangeStatusAsync(graph.Idea.Id, (int)IdeaAccessStatus.RestrictedByAuthor);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var stored = await LoadIdeaAsync(graph.Idea.Id);
        Assert.Equal(IdeaAccessStatus.RestrictedByAuthor, stored.AccessStatus);
        Assert.Null(stored.UpdatedAt);
    }

    [Fact]
    public async Task ChangeStatus_Always_DoesNotTouchOtherIdeasOrThePoll()
    {
        var graph = IdeaIntegrationHelpers.BuildGraph();
        var sibling = IdeaIntegrationHelpers.BuildIdea(graph.Poll.Id);
        await SeedAsync(graph.Entities);
        await SeedAsync(sibling);

        await ChangeStatusAsync(graph.Idea.Id, (int)IdeaAccessStatus.Inactive);

        Assert.Equal(IdeaAccessStatus.Active, (await LoadIdeaAsync(sibling.Id)).AccessStatus);
        var poll = await QueryDbAsync(context => context.Polls.SingleAsync(x => x.Id == graph.Poll.Id));
        Assert.Equal(PollStatus.Active, poll.Status);
        Assert.Null(poll.UpdatedAt);
    }

    [Fact]
    public async Task ChangeStatus_IdeaIsApprovedAndPollIsFinished_StillChangesStatus()
    {
        var graph = IdeaIntegrationHelpers.BuildGraph(
            pollStatus: PollStatus.Finished,
            approvalStatus: IdeaApprovalStatus.Approved);
        await SeedAsync(graph.Entities);

        var response = await ChangeStatusAsync(graph.Idea.Id, (int)IdeaAccessStatus.Inactive);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(IdeaAccessStatus.Inactive, (await LoadIdeaAsync(graph.Idea.Id)).AccessStatus);
    }

    [Fact]
    public async Task ChangeStatus_StatusIsOutsideOfEnum_ReturnsBadRequestAndChangesNothing()
    {
        var graph = IdeaIntegrationHelpers.BuildGraph();
        await SeedAsync(graph.Entities);

        var response = await ChangeStatusAsync(graph.Idea.Id, UnsupportedStatus);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(IdeaAccessStatus.Active, (await LoadIdeaAsync(graph.Idea.Id)).AccessStatus);
    }

    [Fact]
    public async Task ChangeStatus_StatusIsUndefined_StoresIt()
    {
        var graph = IdeaIntegrationHelpers.BuildGraph();
        await SeedAsync(graph.Entities);

        var response = await ChangeStatusAsync(graph.Idea.Id, (int)IdeaAccessStatus.Undefined);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(IdeaAccessStatus.Undefined, (await LoadIdeaAsync(graph.Idea.Id)).AccessStatus);
    }

    [Fact]
    public async Task ChangeStatus_IdeaDoesNotExist_ReturnsNotFound()
    {
        var response = await ChangeStatusAsync(Guid.NewGuid(), (int)IdeaAccessStatus.Inactive);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
