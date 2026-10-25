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

public sealed class ChangeIdeaApprovalStatusCommandHandlerTests(PollsWebAppFactory factory)
    : PollsApiTestBase(factory)
{
    private const int UnsupportedStatus = 99;

    private async Task<HttpResponseMessage> ChangeStatusAsync(Guid id, int newApprovalStatus)
    {
        var client = CreateClient([Permissions.Ideas.ChangeApprovalStatusAny]);

        return await client.PatchAsJsonAsync(
            $"{IdeaIntegrationHelpers.AdminIdeasRoute}/{id}/{IdeaIntegrationHelpers.ApprovalStatusSegment}",
            new { newApprovalStatus });
    }

    private Task<Idea> LoadIdeaAsync(Guid id) =>
        QueryDbAsync(context => context.Ideas.SingleAsync(x => x.Id == id));

    [Theory]
    [MemberData(nameof(ChangeIdeaStatusIntegrationTestData.ApprovalTransitions), MemberType = typeof(ChangeIdeaStatusIntegrationTestData))]
    public async Task ChangeStatus_ValidTransition_StoresNewStatusAndKeepsAccessStatus(
        IdeaApprovalStatus current,
        IdeaApprovalStatus target)
    {
        var graph = IdeaIntegrationHelpers.BuildGraph(
            accessStatus: IdeaAccessStatus.RestrictedByContext,
            approvalStatus: current);
        await SeedAsync(graph.Entities);

        var response = await ChangeStatusAsync(graph.Idea.Id, (int)target);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var stored = await LoadIdeaAsync(graph.Idea.Id);
        Assert.Equal(target, stored.ApprovalStatus);
        Assert.Equal(IdeaAccessStatus.RestrictedByContext, stored.AccessStatus);
    }

    [Fact]
    public async Task ChangeStatus_StatusIsAlreadySet_ReturnsOkAndChangesNothing()
    {
        var graph = IdeaIntegrationHelpers.BuildGraph(approvalStatus: IdeaApprovalStatus.Deferred);
        await SeedAsync(graph.Entities);

        var response = await ChangeStatusAsync(graph.Idea.Id, (int)IdeaApprovalStatus.Deferred);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var stored = await LoadIdeaAsync(graph.Idea.Id);
        Assert.Equal(IdeaApprovalStatus.Deferred, stored.ApprovalStatus);
        Assert.Null(stored.UpdatedAt);
    }

    [Fact]
    public async Task ChangeStatus_Always_DoesNotTouchOtherIdeasOrThePoll()
    {
        var graph = IdeaIntegrationHelpers.BuildGraph();
        var sibling = IdeaIntegrationHelpers.BuildIdea(graph.Poll.Id);
        await SeedAsync(graph.Entities);
        await SeedAsync(sibling);

        await ChangeStatusAsync(graph.Idea.Id, (int)IdeaApprovalStatus.Approved);

        Assert.Equal(IdeaApprovalStatus.Pending, (await LoadIdeaAsync(sibling.Id)).ApprovalStatus);
        var poll = await QueryDbAsync(context => context.Polls.SingleAsync(x => x.Id == graph.Poll.Id));
        Assert.Equal(PollStatus.Active, poll.Status);
        Assert.Null(poll.UpdatedAt);
    }

    [Fact]
    public async Task ChangeStatus_PollIsFinished_StillChangesStatus()
    {
        var graph = IdeaIntegrationHelpers.BuildGraph(pollStatus: PollStatus.Finished);
        await SeedAsync(graph.Entities);

        var response = await ChangeStatusAsync(graph.Idea.Id, (int)IdeaApprovalStatus.Approved);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(IdeaApprovalStatus.Approved, (await LoadIdeaAsync(graph.Idea.Id)).ApprovalStatus);
    }

    [Fact]
    public async Task ChangeStatus_StatusIsOutsideOfEnum_ReturnsBadRequestAndChangesNothing()
    {
        var graph = IdeaIntegrationHelpers.BuildGraph();
        await SeedAsync(graph.Entities);

        var response = await ChangeStatusAsync(graph.Idea.Id, UnsupportedStatus);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(IdeaApprovalStatus.Pending, (await LoadIdeaAsync(graph.Idea.Id)).ApprovalStatus);
    }

    [Fact]
    public async Task ChangeStatus_StatusIsUndefined_ReturnsConflictAndChangesNothing()
    {
        var graph = IdeaIntegrationHelpers.BuildGraph();
        await SeedAsync(graph.Entities);

        var response = await ChangeStatusAsync(graph.Idea.Id, (int)IdeaApprovalStatus.Undefined);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(IdeaApprovalStatus.Pending, (await LoadIdeaAsync(graph.Idea.Id)).ApprovalStatus);
    }

    [Fact]
    public async Task ChangeStatus_IdeaDoesNotExist_ReturnsNotFound()
    {
        var response = await ChangeStatusAsync(Guid.NewGuid(), (int)IdeaApprovalStatus.Approved);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
