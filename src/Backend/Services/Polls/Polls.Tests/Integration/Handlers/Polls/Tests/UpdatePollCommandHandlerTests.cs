using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Moq;
using Polls.Application.Common.Constants;
using Polls.Application.Common.Models;
using Polls.Domain.Authorization;
using Polls.Domain.Cities;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.Infrastructure.Constants;
using Polls.Tests.Integration.Handlers.Cities;
using Polls.Tests.Integration.Handlers.Polls.TestData;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Polls.Tests;

public sealed class UpdatePollCommandHandlerTests(PollsWebAppFactory factory) : PollsApiTestBase(factory)
{
    private const string OriginalTitle = "Park renovation";
    private const string NewTitle = "Updated vote";
    private const string NewDescription = "Where should the money go";
    private const string FirstImageFileName = "first.jpg";
    private const string SecondImageFileName = "second.jpg";
    private const string AddedImageFileName = "added.jpg";
    private const decimal NewBudget = 2500.75m;
    private const int NewEndsInDays = 15;
    private const int ShortEndsInDays = 2;
    private const int ExpiredEditWindowDays = 2;
    private const string ValueProperty = "value";
    private const string TitleProperty = "title";

    private static readonly TimeSpan DateTolerance = TimeSpan.FromMilliseconds(1);

    private sealed record PollGraph(City City, Poll Poll);

    private static DateTimeOffset NewEndsAt => DateTimeOffset.UtcNow.AddDays(NewEndsInDays);

    private async Task<PollGraph> SeedPollAsync(
        PollStatus status = PollStatus.Active,
        DateTimeOffset? createdAt = null,
        string[]? imageFileNames = null)
    {
        var city = CityIntegrationHelpers.BuildCity();
        var poll = PollIntegrationHelpers.BuildPoll(
            city.Id,
            status,
            PollType.ManagerVote,
            imageFileNames ?? []);
        poll.Title = OriginalTitle;
        poll.BudgetAmount = NewBudget + 1;
        await SeedAsync(city, poll);

        if (createdAt is not null)
            await ExecuteDbAsync(context => context.Polls
                .Where(x => x.Id == poll.Id)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.CreatedAt, createdAt.Value)));

        return new PollGraph(city, poll);
    }

    private Task<Poll> LoadPollAsync(Guid id) =>
        QueryDbAsync(context => context.Polls.Include(x => x.Images).SingleAsync(x => x.Id == id));

    private Task<HttpResponseMessage> UpdateAsAdminAsync(Guid id, MultipartFormDataContent form) =>
        CreateClient([Permissions.Polls.UpdateAny])
            .PutAsync($"{PollIntegrationHelpers.AdminPollsRoute}/{id}", form);

    private Task<HttpResponseMessage> UpdateAsUserAsync(Guid id, Guid userCityId, MultipartFormDataContent form) =>
        CreateClient([Permissions.Polls.UpdateCity], cityId: userCityId)
            .PutAsync($"{PollIntegrationHelpers.UserPollsRoute}/{id}", form);

    private static MultipartFormDataContent ValidForm(
        DateTimeOffset? endsAt = null,
        IReadOnlyCollection<string>? imageFileNamesToAdd = null,
        IReadOnlyCollection<Guid>? imageIdsToDelete = null) =>
        PollIntegrationHelpers.BuildUpdateForm(
            NewTitle,
            endsAt ?? NewEndsAt,
            NewBudget,
            NewDescription,
            imageFileNamesToAdd,
            imageIdsToDelete);

    private void VerifyUploadCalls(Times times) =>
        Factory.ImageStorage.Verify(
            x => x.UploadRangeAsync(It.IsAny<IReadOnlyList<ImageFile>>(), It.IsAny<CancellationToken>()),
            times);

    private void VerifyScheduleCalls(Times times) =>
        Factory.PollScheduler.Verify(
            x => x.ScheduleAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()),
            times);

    [Fact]
    public async Task Handle_ValidRequest_UpdatesFieldsAndKeepsTypeStatusAndCity()
    {
        var graph = await SeedPollAsync();
        var endsAt = NewEndsAt;

        var response = await UpdateAsAdminAsync(graph.Poll.Id, ValidForm(endsAt));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var stored = await LoadPollAsync(graph.Poll.Id);
        Assert.Equal(NewTitle, stored.Title);
        Assert.Equal(NewDescription, stored.Description);
        Assert.Equal(NewBudget, stored.BudgetAmount);
        Assert.True((stored.EndsAt - endsAt).Duration() < DateTolerance);
        Assert.Equal(PollType.ManagerVote, stored.Type);
        Assert.Equal(PollStatus.Active, stored.Status);
        Assert.Equal(graph.City.Id, stored.CityId);
        var returnedTitle = JsonDocument
            .Parse(await response.Content.ReadAsStringAsync())
            .RootElement
            .GetProperty(ValueProperty)
            .GetProperty(TitleProperty)
            .GetString();
        Assert.Equal(NewTitle, returnedTitle);
    }

    [Fact]
    public async Task Handle_ImagesToAdd_UploadsThemOnceAndStoresAfterExistingOnes()
    {
        var graph = await SeedPollAsync(imageFileNames: [FirstImageFileName]);

        var response = await UpdateAsAdminAsync(
            graph.Poll.Id,
            ValidForm(imageFileNamesToAdd: [AddedImageFileName, SecondImageFileName]));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var images = (await LoadPollAsync(graph.Poll.Id)).Images.OrderBy(x => x.Order).ToList();
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
        var graph = await SeedPollAsync(imageFileNames: [FirstImageFileName, SecondImageFileName]);
        var removed = graph.Poll.Images.Single(x => x.FileName == FirstImageFileName);

        var response = await UpdateAsAdminAsync(graph.Poll.Id, ValidForm(imageIdsToDelete: [removed.Id]));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal([SecondImageFileName], (await LoadPollAsync(graph.Poll.Id)).Images.Select(x => x.FileName));
        var deleted = await QueryDbAsync(context => context.DeletedImages.ToListAsync());
        Assert.Equal([FirstImageFileName], deleted.Select(x => x.FileName));
        VerifyUploadCalls(Times.Never());
    }

    [Fact]
    public async Task Handle_ImageBelongsToAnotherPoll_ReturnsConflictAndChangesNothing()
    {
        var graph = await SeedPollAsync(imageFileNames: [FirstImageFileName]);
        var other = await SeedPollAsync(imageFileNames: [SecondImageFileName]);

        var response = await UpdateAsAdminAsync(
            graph.Poll.Id,
            ValidForm(imageIdsToDelete: [other.Poll.Images.Single().Id]));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(OriginalTitle, (await LoadPollAsync(graph.Poll.Id)).Title);
        Assert.Single((await LoadPollAsync(other.Poll.Id)).Images);
        Assert.Empty(await QueryDbAsync(context => context.DeletedImages.ToListAsync()));
    }

    [Fact]
    public async Task Handle_EndDateChangesOnActivePoll_SchedulesFinishAtNewDate()
    {
        var graph = await SeedPollAsync();
        var endsAt = NewEndsAt;

        await UpdateAsAdminAsync(graph.Poll.Id, ValidForm(endsAt));

        Factory.PollScheduler.Verify(
            x => x.ScheduleAsync(graph.Poll.Id, endsAt, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_EndDateIsUnchanged_DoesNotReschedule()
    {
        var graph = await SeedPollAsync();
        var storedEndsAt = (await LoadPollAsync(graph.Poll.Id)).EndsAt;

        var response = await UpdateAsAdminAsync(graph.Poll.Id, ValidForm(storedEndsAt));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        VerifyScheduleCalls(Times.Never());
    }

    [Fact]
    public async Task Handle_EndDateChangesOnNotActivePoll_DoesNotReschedule()
    {
        var graph = await SeedPollAsync(PollStatus.Suspended);

        var response = await UpdateAsAdminAsync(graph.Poll.Id, ValidForm());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        VerifyScheduleCalls(Times.Never());
    }

    [Theory]
    [MemberData(nameof(UpdatePollIntegrationTestData.InvalidInputs), MemberType = typeof(UpdatePollIntegrationTestData))]
    public async Task Handle_InputIsInvalid_ReturnsBadRequestAndChangesNothing(InvalidUpdateCase invalid)
    {
        var graph = await SeedPollAsync();
        var form = PollIntegrationHelpers.BuildUpdateForm(
            invalid.Title,
            DateTimeOffset.UtcNow.AddDays(invalid.EndsInDays),
            invalid.Budget,
            invalid.Description);

        var response = await UpdateAsAdminAsync(graph.Poll.Id, form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(OriginalTitle, (await LoadPollAsync(graph.Poll.Id)).Title);
        VerifyScheduleCalls(Times.Never());
    }

    [Fact]
    public async Task Handle_PollDoesNotExist_ReturnsNotFound()
    {
        var response = await UpdateAsAdminAsync(Guid.NewGuid(), ValidForm());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Handle_UserUpdatesPollOfOwnCity_UpdatesIt()
    {
        var graph = await SeedPollAsync();

        var response = await UpdateAsUserAsync(graph.Poll.Id, graph.City.Id, ValidForm());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(NewTitle, (await LoadPollAsync(graph.Poll.Id)).Title);
    }

    [Fact]
    public async Task Handle_UserUpdatesPollOfAnotherCity_ReturnsForbiddenAndChangesNothing()
    {
        var graph = await SeedPollAsync();

        var response = await UpdateAsUserAsync(graph.Poll.Id, Guid.NewGuid(), ValidForm());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(OriginalTitle, (await LoadPollAsync(graph.Poll.Id)).Title);
    }

    [Theory]
    [InlineData(PollStatus.Suspended)]
    [InlineData(PollStatus.Finished)]
    public async Task Handle_UserUpdatesPollThatIsNotOpen_ReturnsConflictAndChangesNothing(PollStatus status)
    {
        var graph = await SeedPollAsync(status);

        var response = await UpdateAsUserAsync(graph.Poll.Id, graph.City.Id, ValidForm());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(OriginalTitle, (await LoadPollAsync(graph.Poll.Id)).Title);
    }

    [Fact]
    public async Task Handle_UserUpdatesPollAfterEditWindow_ReturnsConflictAndChangesNothing()
    {
        var graph = await SeedPollAsync(createdAt: DateTimeOffset.UtcNow.AddDays(-ExpiredEditWindowDays));

        var response = await UpdateAsUserAsync(graph.Poll.Id, graph.City.Id, ValidForm());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(OriginalTitle, (await LoadPollAsync(graph.Poll.Id)).Title);
    }

    [Fact]
    public async Task Handle_UserExtendsPollBeyondMaxDuration_ReturnsConflictAndChangesNothing()
    {
        var graph = await SeedPollAsync();
        var endsAt = DateTimeOffset.UtcNow.AddDays(ValidationConstants.Poll.MaxDurationDays + 1);

        var response = await UpdateAsUserAsync(graph.Poll.Id, graph.City.Id, ValidForm(endsAt));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(OriginalTitle, (await LoadPollAsync(graph.Poll.Id)).Title);
    }

    [Fact]
    public async Task Handle_UserShortensPollBelowMinCreateDuration_UpdatesIt()
    {
        var graph = await SeedPollAsync();

        var response = await UpdateAsUserAsync(
            graph.Poll.Id,
            graph.City.Id,
            ValidForm(DateTimeOffset.UtcNow.AddDays(ShortEndsInDays)));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Handle_AdminUpdatesRestrictedPoll_BypassesCityStatusEditWindowAndDurationChecks()
    {
        var graph = await SeedPollAsync(
            PollStatus.Finished,
            DateTimeOffset.UtcNow.AddDays(-ExpiredEditWindowDays));
        var endsAt = DateTimeOffset.UtcNow.AddDays(ValidationConstants.Poll.MaxDurationDays + 1);
        var client = CreateClient([Permissions.Polls.UpdateAny], cityId: Guid.NewGuid());

        var response = await client.PutAsync(
            $"{PollIntegrationHelpers.AdminPollsRoute}/{graph.Poll.Id}",
            ValidForm(endsAt));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(NewTitle, (await LoadPollAsync(graph.Poll.Id)).Title);
    }
}
