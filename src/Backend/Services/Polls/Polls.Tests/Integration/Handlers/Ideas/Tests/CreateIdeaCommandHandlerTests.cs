using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Moq;
using Polls.Application.Common.Models;
using Polls.Domain.Authorization;
using Polls.Domain.Cities;
using Polls.Domain.Ideas.Enums;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.Infrastructure.Constants;
using Polls.Tests.Integration.Handlers.Cities;
using Polls.Tests.Integration.Handlers.Ideas.TestData;
using Polls.Tests.Integration.Handlers.Polls;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Ideas.Tests;

public sealed class CreateIdeaCommandHandlerTests(PollsWebAppFactory factory) : PollsApiTestBase(factory)
{
    private const string IdeaTitle = "More benches";
    private const string IdeaDescription = "Along the river";
    private const string SecondImageFileName = "second.jpg";
    private const int ExpiredDays = -1;
    private const string ValueProperty = "value";
    private const string IdProperty = "id";

    private sealed record PollInCity(City City, Poll Poll);

    private async Task<PollInCity> SeedPollAsync(PollStatus status = PollStatus.Active)
    {
        var city = CityIntegrationHelpers.BuildCity();
        var poll = PollIntegrationHelpers.BuildPoll(city.Id, status);
        await SeedAsync(city, poll);

        return new PollInCity(city, poll);
    }

    private Task<HttpResponseMessage> CreateAsUserAsync(
        Guid pollId,
        Guid userId,
        Guid userCityId,
        MultipartFormDataContent form) =>
        CreateClient([Permissions.Ideas.CreateCity], userId, userCityId)
            .PostAsync(
                $"{IdeaIntegrationHelpers.UserIdeasRoute}/{IdeaIntegrationHelpers.PollsSegment}/{pollId}",
                form);

    private Task<HttpResponseMessage> CreateAsAdminAsync(Guid pollId, Guid adminId, MultipartFormDataContent form) =>
        CreateClient([Permissions.Ideas.CreateAny], adminId)
            .PostAsync($"{IdeaIntegrationHelpers.AdminIdeasRoute}/{pollId}", form);

    private static MultipartFormDataContent ValidForm(IReadOnlyCollection<string>? imageFileNames = null) =>
        IdeaIntegrationHelpers.BuildCreateForm(IdeaTitle, IdeaDescription, imageFileNames);

    private void VerifyUploadCalls(Times times) =>
        Factory.ImageStorage.Verify(
            x => x.UploadRangeAsync(It.IsAny<IReadOnlyList<ImageFile>>(), It.IsAny<CancellationToken>()),
            times);

    [Fact]
    public async Task Handle_ValidRequest_StoresActivePendingIdeaOwnedByCaller()
    {
        var target = await SeedPollAsync();
        var userId = Guid.NewGuid();

        var response = await CreateAsUserAsync(target.Poll.Id, userId, target.City.Id, ValidForm());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var stored = await QueryDbAsync(context => context.Ideas.SingleAsync());
        Assert.Equal(userId, stored.UserId);
        Assert.Equal(target.Poll.Id, stored.PollId);
        Assert.Equal(IdeaTitle, stored.Title);
        Assert.Equal(IdeaDescription, stored.Description);
        Assert.Equal(IdeaAccessStatus.Active, stored.AccessStatus);
        Assert.Equal(IdeaApprovalStatus.Pending, stored.ApprovalStatus);
        var returnedId = JsonDocument
            .Parse(await response.Content.ReadAsStringAsync())
            .RootElement
            .GetProperty(ValueProperty)
            .GetProperty(IdProperty)
            .GetGuid();
        Assert.Equal(stored.Id, returnedId);
    }

    [Fact]
    public async Task Handle_WithImages_StoresThemInOrderAndUploadsOnce()
    {
        var target = await SeedPollAsync();

        var response = await CreateAsUserAsync(
            target.Poll.Id,
            Guid.NewGuid(),
            target.City.Id,
            ValidForm([IdeaIntegrationHelpers.ImageFileName, SecondImageFileName]));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var images = await QueryDbAsync(context => context.IdeaImages.OrderBy(x => x.Order).ToListAsync());
        Assert.Equal([0, 1], images.Select(x => x.Order));
        Assert.All(images, image => Assert.StartsWith(EnvironmentConstants.ImageStorage.UploadedPrefix, image.FileName));
        VerifyUploadCalls(Times.Once());
    }

    [Fact]
    public async Task Handle_WithoutImages_StoresNoImagesAndDoesNotUpload()
    {
        var target = await SeedPollAsync();

        await CreateAsUserAsync(target.Poll.Id, Guid.NewGuid(), target.City.Id, ValidForm());

        Assert.Empty(await QueryDbAsync(context => context.IdeaImages.ToListAsync()));
        VerifyUploadCalls(Times.Never());
    }

    [Fact]
    public async Task Handle_SameUserCreatesTwoIdeas_StoresBoth()
    {
        var target = await SeedPollAsync();
        var userId = Guid.NewGuid();

        await CreateAsUserAsync(target.Poll.Id, userId, target.City.Id, ValidForm());
        await CreateAsUserAsync(target.Poll.Id, userId, target.City.Id, ValidForm());

        Assert.Equal(2, await QueryDbAsync(context => context.Ideas.CountAsync()));
    }

    [Fact]
    public async Task Handle_PollDoesNotExist_ReturnsNotFound()
    {
        var response = await CreateAsAdminAsync(Guid.NewGuid(), Guid.NewGuid(), ValidForm());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Empty(await QueryDbAsync(context => context.Ideas.ToListAsync()));
    }

    [Theory]
    [MemberData(nameof(CreateIdeaIntegrationTestData.InvalidInputs), MemberType = typeof(CreateIdeaIntegrationTestData))]
    public async Task Handle_InputIsInvalid_ReturnsBadRequestAndStoresNothing(InvalidIdeaCase invalid)
    {
        var target = await SeedPollAsync();
        var form = IdeaIntegrationHelpers.BuildCreateForm(invalid.Title, invalid.Description);

        var response = await CreateAsUserAsync(target.Poll.Id, Guid.NewGuid(), target.City.Id, form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(await QueryDbAsync(context => context.Ideas.ToListAsync()));
    }

    [Fact]
    public async Task Handle_UserCreatesIdeaInPollOfAnotherCity_ReturnsForbiddenAndStoresNothing()
    {
        var target = await SeedPollAsync();

        var response = await CreateAsUserAsync(target.Poll.Id, Guid.NewGuid(), Guid.NewGuid(), ValidForm());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Empty(await QueryDbAsync(context => context.Ideas.ToListAsync()));
    }

    [Theory]
    [InlineData(PollStatus.Suspended)]
    [InlineData(PollStatus.Finished)]
    public async Task Handle_UserCreatesIdeaInPollThatIsNotOpen_ReturnsConflictAndStoresNothing(PollStatus status)
    {
        var target = await SeedPollAsync(status);

        var response = await CreateAsUserAsync(target.Poll.Id, Guid.NewGuid(), target.City.Id, ValidForm());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Empty(await QueryDbAsync(context => context.Ideas.ToListAsync()));
    }

    [Fact]
    public async Task Handle_UserCreatesIdeaInPollPastItsEndDate_ReturnsConflictAndStoresNothing()
    {
        var target = await SeedPollAsync();
        await ExecuteDbAsync(context => context.Polls
            .Where(x => x.Id == target.Poll.Id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.EndsAt, DateTimeOffset.UtcNow.AddDays(ExpiredDays))));

        var response = await CreateAsUserAsync(target.Poll.Id, Guid.NewGuid(), target.City.Id, ValidForm());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Empty(await QueryDbAsync(context => context.Ideas.ToListAsync()));
    }

    [Fact]
    public async Task Handle_AdminCreatesIdea_StoresItOwnedByAdmin()
    {
        var target = await SeedPollAsync();
        var adminId = Guid.NewGuid();

        var response = await CreateAsAdminAsync(target.Poll.Id, adminId, ValidForm());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(adminId, (await QueryDbAsync(context => context.Ideas.SingleAsync())).UserId);
    }

    [Theory]
    [InlineData(PollStatus.Suspended)]
    [InlineData(PollStatus.Finished)]
    public async Task Handle_AdminCreatesIdeaInPollThatIsNotOpen_StoresIt(PollStatus status)
    {
        var target = await SeedPollAsync(status);

        var response = await CreateAsAdminAsync(target.Poll.Id, Guid.NewGuid(), ValidForm());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, await QueryDbAsync(context => context.Ideas.CountAsync()));
    }
}
