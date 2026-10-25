using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Moq;
using Polls.Application.Common.Models;
using Polls.Domain.Authorization;
using Polls.Domain.Ideas;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.Infrastructure.Constants;
using Polls.Tests.Integration.Handlers.Ideas.TestData;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Ideas.Tests;

public sealed class UpdateIdeaCommandHandlerTests(PollsWebAppFactory factory) : PollsApiTestBase(factory)
{
    private const string NewTitle = "Updated idea";
    private const string NewDescription = "Along the river";
    private const string FirstImageFileName = "first.jpg";
    private const string SecondImageFileName = "second.jpg";
    private const string AddedImageFileName = "added.jpg";
    private const string ValueProperty = "value";
    private const string TitleProperty = "title";

    private Task<Idea> LoadIdeaAsync(Guid id) =>
        QueryDbAsync(context => context.Ideas.Include(x => x.Images).SingleAsync(x => x.Id == id));

    private Task<Poll> LoadPollAsync(Guid id) =>
        QueryDbAsync(context => context.Polls.SingleAsync(x => x.Id == id));

    private Task<HttpResponseMessage> UpdateAsUserAsync(Guid id, Guid userId, MultipartFormDataContent form) =>
        CreateClient([Permissions.Ideas.UpdateOwn], userId)
            .PutAsync($"{IdeaIntegrationHelpers.UserIdeasRoute}/{id}", form);

    private Task<HttpResponseMessage> UpdateAsAdminAsync(Guid id, MultipartFormDataContent form) =>
        CreateClient([Permissions.Ideas.UpdateAny], Guid.NewGuid())
            .PutAsync($"{IdeaIntegrationHelpers.AdminIdeasRoute}/{id}", form);

    private static MultipartFormDataContent ValidForm(
        IReadOnlyCollection<string>? imageFileNamesToAdd = null,
        IReadOnlyCollection<Guid>? imageIdsToDelete = null) =>
        IdeaIntegrationHelpers.BuildUpdateForm(NewTitle, NewDescription, imageFileNamesToAdd, imageIdsToDelete);

    private void VerifyUploadCalls(Times times) =>
        Factory.ImageStorage.Verify(
            x => x.UploadRangeAsync(It.IsAny<IReadOnlyList<ImageFile>>(), It.IsAny<CancellationToken>()),
            times);

    [Fact]
    public async Task Handle_ValidRequest_UpdatesFieldsAndKeepsOwnerAndStatuses()
    {
        var owner = Guid.NewGuid();
        var graph = IdeaIntegrationHelpers.BuildGraph(
            owner,
            accessStatus: IdeaAccessStatus.RestrictedByAuthor,
            approvalStatus: IdeaApprovalStatus.Rejected);
        await SeedAsync(graph.Entities);

        var response = await UpdateAsUserAsync(graph.Idea.Id, owner, ValidForm());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var stored = await LoadIdeaAsync(graph.Idea.Id);
        Assert.Equal(NewTitle, stored.Title);
        Assert.Equal(NewDescription, stored.Description);
        Assert.Equal(owner, stored.UserId);
        Assert.Equal(graph.Poll.Id, stored.PollId);
        Assert.Equal(IdeaAccessStatus.RestrictedByAuthor, stored.AccessStatus);
        Assert.Equal(IdeaApprovalStatus.Rejected, stored.ApprovalStatus);
        var returnedTitle = JsonDocument
            .Parse(await response.Content.ReadAsStringAsync())
            .RootElement
            .GetProperty(ValueProperty)
            .GetProperty(TitleProperty)
            .GetString();
        Assert.Equal(NewTitle, returnedTitle);
    }

    [Fact]
    public async Task Handle_ValidRequest_DoesNotTouchThePoll()
    {
        var owner = Guid.NewGuid();
        var graph = IdeaIntegrationHelpers.BuildGraph(owner);
        await SeedAsync(graph.Entities);
        var pollBefore = await LoadPollAsync(graph.Poll.Id);

        await UpdateAsUserAsync(graph.Idea.Id, owner, ValidForm());

        var pollAfter = await LoadPollAsync(graph.Poll.Id);
        Assert.Equal(pollBefore.Title, pollAfter.Title);
        Assert.Equal(pollBefore.Status, pollAfter.Status);
        Assert.Equal(pollBefore.UpdatedAt, pollAfter.UpdatedAt);
    }

    [Fact]
    public async Task Handle_ImagesToAdd_UploadsThemOnceAndStoresAfterExistingOnes()
    {
        var owner = Guid.NewGuid();
        var graph = IdeaIntegrationHelpers.BuildGraph(owner, imageFileNames: [FirstImageFileName]);
        await SeedAsync(graph.Entities);

        var response = await UpdateAsUserAsync(
            graph.Idea.Id,
            owner,
            ValidForm(imageFileNamesToAdd: [AddedImageFileName, SecondImageFileName]));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var images = (await LoadIdeaAsync(graph.Idea.Id)).Images.OrderBy(x => x.Order).ToList();
        Assert.Equal([0, 1, 2], images.Select(x => x.Order));
        Assert.Equal(FirstImageFileName, images[0].FileName);
        Assert.All(
            images.Skip(1),
            image => Assert.StartsWith(EnvironmentConstants.ImageStorage.UploadedPrefix, image.FileName));
        VerifyUploadCalls(Times.Once());
    }

    [Fact]
    public async Task Handle_ImagesToDelete_RemovesThemAndQueuesFilesForDeletion()
    {
        var owner = Guid.NewGuid();
        var graph = IdeaIntegrationHelpers.BuildGraph(
            owner,
            imageFileNames: [FirstImageFileName, SecondImageFileName]);
        await SeedAsync(graph.Entities);
        var removed = graph.Idea.Images.Single(x => x.FileName == FirstImageFileName);

        var response = await UpdateAsUserAsync(graph.Idea.Id, owner, ValidForm(imageIdsToDelete: [removed.Id]));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal([SecondImageFileName], (await LoadIdeaAsync(graph.Idea.Id)).Images.Select(x => x.FileName));
        var deleted = await QueryDbAsync(context => context.DeletedImages.ToListAsync());
        Assert.Equal([FirstImageFileName], deleted.Select(x => x.FileName));
        VerifyUploadCalls(Times.Never());
    }

    [Fact]
    public async Task Handle_ImageBelongsToAnotherIdea_ReturnsConflictAndChangesNothing()
    {
        var owner = Guid.NewGuid();
        var graph = IdeaIntegrationHelpers.BuildGraph(owner, imageFileNames: [FirstImageFileName]);
        var other = IdeaIntegrationHelpers.BuildGraph(imageFileNames: [SecondImageFileName]);
        await SeedAsync(graph.Entities);
        await SeedAsync(other.Entities);

        var response = await UpdateAsUserAsync(
            graph.Idea.Id,
            owner,
            ValidForm(imageIdsToDelete: [other.Idea.Images.Single().Id]));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(graph.Idea.Title, (await LoadIdeaAsync(graph.Idea.Id)).Title);
        Assert.Single((await LoadIdeaAsync(other.Idea.Id)).Images);
        Assert.Empty(await QueryDbAsync(context => context.DeletedImages.ToListAsync()));
    }

    [Theory]
    [MemberData(nameof(CreateIdeaIntegrationTestData.InvalidInputs), MemberType = typeof(CreateIdeaIntegrationTestData))]
    public async Task Handle_InputIsInvalid_ReturnsBadRequestAndChangesNothing(InvalidIdeaCase invalid)
    {
        var owner = Guid.NewGuid();
        var graph = IdeaIntegrationHelpers.BuildGraph(owner);
        await SeedAsync(graph.Entities);

        var response = await UpdateAsUserAsync(
            graph.Idea.Id,
            owner,
            IdeaIntegrationHelpers.BuildUpdateForm(invalid.Title, invalid.Description));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(graph.Idea.Title, (await LoadIdeaAsync(graph.Idea.Id)).Title);
    }

    [Fact]
    public async Task Handle_IdeaDoesNotExist_ReturnsNotFound()
    {
        var response = await UpdateAsUserAsync(Guid.NewGuid(), Guid.NewGuid(), ValidForm());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Handle_UserIsNotTheOwner_ReturnsForbiddenAndChangesNothing()
    {
        var graph = IdeaIntegrationHelpers.BuildGraph();
        await SeedAsync(graph.Entities);

        var response = await UpdateAsUserAsync(graph.Idea.Id, Guid.NewGuid(), ValidForm());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(graph.Idea.Title, (await LoadIdeaAsync(graph.Idea.Id)).Title);
    }

    [Fact]
    public async Task Handle_IdeaIsApproved_ReturnsConflictAndChangesNothing()
    {
        var owner = Guid.NewGuid();
        var graph = IdeaIntegrationHelpers.BuildGraph(owner, approvalStatus: IdeaApprovalStatus.Approved);
        await SeedAsync(graph.Entities);

        var response = await UpdateAsUserAsync(graph.Idea.Id, owner, ValidForm());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(graph.Idea.Title, (await LoadIdeaAsync(graph.Idea.Id)).Title);
    }

    [Theory]
    [MemberData(nameof(UpdateIdeaIntegrationTestData.EditableApprovalStatuses), MemberType = typeof(UpdateIdeaIntegrationTestData))]
    public async Task Handle_IdeaIsNotApproved_AllowsOwnerToUpdateIt(IdeaApprovalStatus approvalStatus)
    {
        var owner = Guid.NewGuid();
        var graph = IdeaIntegrationHelpers.BuildGraph(owner, approvalStatus: approvalStatus);
        await SeedAsync(graph.Entities);

        var response = await UpdateAsUserAsync(graph.Idea.Id, owner, ValidForm());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData(PollStatus.Suspended)]
    [InlineData(PollStatus.Finished)]
    public async Task Handle_PollIsNotOpen_ReturnsConflictAndChangesNothing(PollStatus status)
    {
        var owner = Guid.NewGuid();
        var graph = IdeaIntegrationHelpers.BuildGraph(owner, status);
        await SeedAsync(graph.Entities);

        var response = await UpdateAsUserAsync(graph.Idea.Id, owner, ValidForm());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(graph.Idea.Title, (await LoadIdeaAsync(graph.Idea.Id)).Title);
    }

    [Fact]
    public async Task Handle_AdminUpdatesRestrictedIdea_BypassesOwnerApprovalAndPollChecks()
    {
        var graph = IdeaIntegrationHelpers.BuildGraph(
            pollStatus: PollStatus.Finished,
            approvalStatus: IdeaApprovalStatus.Approved);
        await SeedAsync(graph.Entities);

        var response = await UpdateAsAdminAsync(graph.Idea.Id, ValidForm());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(NewTitle, (await LoadIdeaAsync(graph.Idea.Id)).Title);
    }
}
