using System.Net;
using Microsoft.EntityFrameworkCore;
using Polls.Domain.Authorization;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Polls.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.Integration.Handlers.Ideas.TestData;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Ideas.Tests;

public sealed class DeleteIdeaCommandHandlerTests(PollsWebAppFactory factory) : PollsApiTestBase(factory)
{
    private const string FirstImageFileName = "first.jpg";
    private const string SecondImageFileName = "second.jpg";

    private Task<int> CountIdeasAsync(Guid id) =>
        QueryDbAsync(context => context.Ideas.CountAsync(x => x.Id == id));

    private Task<HttpResponseMessage> DeleteAsUserAsync(Guid id, Guid userId) =>
        CreateClient([Permissions.Ideas.DeleteOwn], userId)
            .DeleteAsync($"{IdeaIntegrationHelpers.UserIdeasRoute}/{id}");

    private Task<HttpResponseMessage> DeleteAsAdminAsync(Guid id) =>
        CreateClient([Permissions.Ideas.DeleteAny], Guid.NewGuid())
            .DeleteAsync($"{IdeaIntegrationHelpers.AdminIdeasRoute}/{id}");

    [Fact]
    public async Task Handle_OwnerDeletesIdea_RemovesIt()
    {
        var owner = Guid.NewGuid();
        var graph = IdeaIntegrationHelpers.BuildGraph(owner);
        await SeedAsync(graph.Entities);

        var response = await DeleteAsUserAsync(graph.Idea.Id, owner);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, await CountIdeasAsync(graph.Idea.Id));
    }

    [Fact]
    public async Task Handle_Always_KeepsThePollAndItsOtherIdeas()
    {
        var owner = Guid.NewGuid();
        var graph = IdeaIntegrationHelpers.BuildGraph(owner);
        var sibling = IdeaIntegrationHelpers.BuildIdea(graph.Poll.Id);
        await SeedAsync(graph.Entities);
        await SeedAsync(sibling);

        await DeleteAsUserAsync(graph.Idea.Id, owner);

        Assert.Equal(1, await QueryDbAsync(context => context.Polls.CountAsync(x => x.Id == graph.Poll.Id)));
        Assert.Equal(1, await CountIdeasAsync(sibling.Id));
    }

    [Fact]
    public async Task Handle_Always_DoesNotModifyThePoll()
    {
        var owner = Guid.NewGuid();
        var graph = IdeaIntegrationHelpers.BuildGraph(owner);
        await SeedAsync(graph.Entities);
        var pollBefore = await QueryDbAsync(context => context.Polls.SingleAsync(x => x.Id == graph.Poll.Id));

        await DeleteAsUserAsync(graph.Idea.Id, owner);

        var pollAfter = await QueryDbAsync(context => context.Polls.SingleAsync(x => x.Id == graph.Poll.Id));
        Assert.Equal(pollBefore.Title, pollAfter.Title);
        Assert.Equal(pollBefore.UpdatedAt, pollAfter.UpdatedAt);
    }

    [Fact]
    public async Task Handle_IdeaHasImages_RemovesThemAndQueuesFilesForDeletion()
    {
        var owner = Guid.NewGuid();
        var graph = IdeaIntegrationHelpers.BuildGraph(
            owner,
            imageFileNames: [FirstImageFileName, SecondImageFileName]);
        await SeedAsync(graph.Entities);

        await DeleteAsUserAsync(graph.Idea.Id, owner);

        Assert.Equal(0, await QueryDbAsync(context => context.IdeaImages.CountAsync()));
        var queued = await QueryDbAsync(context => context.DeletedImages.Select(x => x.FileName).ToListAsync());
        Assert.Equivalent(new[] { FirstImageFileName, SecondImageFileName }, queued);
    }

    [Fact]
    public async Task Handle_Always_DoesNotTouchOtherIdeasOrTheirImages()
    {
        var owner = Guid.NewGuid();
        var graph = IdeaIntegrationHelpers.BuildGraph(owner, imageFileNames: [FirstImageFileName]);
        var other = IdeaIntegrationHelpers.BuildGraph(imageFileNames: [SecondImageFileName]);
        await SeedAsync(graph.Entities);
        await SeedAsync(other.Entities);

        await DeleteAsUserAsync(graph.Idea.Id, owner);

        Assert.Equal(1, await CountIdeasAsync(other.Idea.Id));
        Assert.Equal(1, await QueryDbAsync(context => context.IdeaImages.CountAsync(x => x.IdeaId == other.Idea.Id)));
    }

    [Fact]
    public async Task Handle_IdeaDoesNotExist_ReturnsNotFound()
    {
        var response = await DeleteAsUserAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Handle_IdeaIsDeletedTwice_ReturnsNotFoundTheSecondTime()
    {
        var owner = Guid.NewGuid();
        var graph = IdeaIntegrationHelpers.BuildGraph(owner);
        await SeedAsync(graph.Entities);
        await DeleteAsUserAsync(graph.Idea.Id, owner);

        var response = await DeleteAsUserAsync(graph.Idea.Id, owner);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Handle_UserIsNotTheOwner_ReturnsForbiddenAndKeepsIdea()
    {
        var graph = IdeaIntegrationHelpers.BuildGraph();
        await SeedAsync(graph.Entities);

        var response = await DeleteAsUserAsync(graph.Idea.Id, Guid.NewGuid());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(1, await CountIdeasAsync(graph.Idea.Id));
    }

    [Fact]
    public async Task Handle_IdeaIsApproved_ReturnsConflictAndKeepsIdea()
    {
        var owner = Guid.NewGuid();
        var graph = IdeaIntegrationHelpers.BuildGraph(owner, approvalStatus: IdeaApprovalStatus.Approved);
        await SeedAsync(graph.Entities);

        var response = await DeleteAsUserAsync(graph.Idea.Id, owner);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(1, await CountIdeasAsync(graph.Idea.Id));
    }

    [Theory]
    [MemberData(nameof(UpdateIdeaIntegrationTestData.EditableApprovalStatuses), MemberType = typeof(UpdateIdeaIntegrationTestData))]
    public async Task Handle_IdeaIsNotApproved_AllowsOwnerToDeleteIt(IdeaApprovalStatus approvalStatus)
    {
        var owner = Guid.NewGuid();
        var graph = IdeaIntegrationHelpers.BuildGraph(owner, approvalStatus: approvalStatus);
        await SeedAsync(graph.Entities);

        var response = await DeleteAsUserAsync(graph.Idea.Id, owner);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, await CountIdeasAsync(graph.Idea.Id));
    }

    [Theory]
    [InlineData(PollStatus.Suspended)]
    [InlineData(PollStatus.Finished)]
    public async Task Handle_PollIsNotOpen_ReturnsConflictAndKeepsIdea(PollStatus status)
    {
        var owner = Guid.NewGuid();
        var graph = IdeaIntegrationHelpers.BuildGraph(owner, status);
        await SeedAsync(graph.Entities);

        var response = await DeleteAsUserAsync(graph.Idea.Id, owner);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(1, await CountIdeasAsync(graph.Idea.Id));
    }

    [Fact]
    public async Task Handle_AdminDeletesRestrictedIdea_BypassesOwnerApprovalAndPollChecks()
    {
        var graph = IdeaIntegrationHelpers.BuildGraph(
            pollStatus: PollStatus.Finished,
            approvalStatus: IdeaApprovalStatus.Approved);
        await SeedAsync(graph.Entities);

        var response = await DeleteAsAdminAsync(graph.Idea.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, await CountIdeasAsync(graph.Idea.Id));
    }
}
