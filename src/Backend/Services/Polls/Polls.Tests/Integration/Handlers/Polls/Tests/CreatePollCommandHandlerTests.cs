using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Moq;
using Polls.Application.Common.Models;
using Polls.Domain.Authorization;
using Polls.Domain.Cities;
using Polls.Domain.Polls.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.Infrastructure.Constants;
using Polls.Tests.Integration.Handlers.Cities;
using Polls.Tests.Integration.Handlers.Polls.TestData;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Polls.Tests;

public sealed class CreatePollCommandHandlerTests(PollsWebAppFactory factory) : PollsApiTestBase(factory)
{
    private const string PollTitle = "Budget vote";
    private const string PollDescription = "Where should the money go";
    private const string SecondImageFileName = "second.jpg";
    private const decimal Budget = 1500.5m;
    private const int EndsInDays = 10;
    private const string ValueProperty = "value";
    private const string IdProperty = "id";

    private static readonly TimeSpan DateTolerance = TimeSpan.FromMilliseconds(1);

    private static DateTimeOffset EndsAt => DateTimeOffset.UtcNow.AddDays(EndsInDays);

    private async Task<City> SeedCityAsync()
    {
        var city = CityIntegrationHelpers.BuildCity();
        await SeedAsync(city);

        return city;
    }

    private Task<HttpResponseMessage> CreateAsAdminAsync(Guid cityId, MultipartFormDataContent form) =>
        CreateClient([Permissions.Polls.CreateAny])
            .PostAsync($"{PollIntegrationHelpers.AdminPollsRoute}/{PollIntegrationHelpers.CitiesSegment}/{cityId}", form);

    private Task<HttpResponseMessage> CreateAsUserAsync(Guid cityId, Guid userCityId, MultipartFormDataContent form) =>
        CreateClient([Permissions.Polls.CreateCity], cityId: userCityId)
            .PostAsync($"{PollIntegrationHelpers.UserPollsRoute}/{PollIntegrationHelpers.CitiesSegment}/{cityId}", form);

    private static MultipartFormDataContent ValidForm(
        PollType type = PollType.UserVote,
        DateTimeOffset? endsAt = null,
        IReadOnlyCollection<string>? imageFileNames = null) =>
        PollIntegrationHelpers.BuildCreateForm(
            PollTitle,
            type,
            endsAt ?? EndsAt,
            Budget,
            PollDescription,
            imageFileNames);

    private void VerifyUploadCalls(Times times) =>
        Factory.ImageStorage.Verify(
            x => x.UploadRangeAsync(It.IsAny<IReadOnlyList<ImageFile>>(), It.IsAny<CancellationToken>()),
            times);

    [Fact]
    public async Task Handle_ValidRequest_StoresActivePollWithGivenData()
    {
        var city = await SeedCityAsync();
        var endsAt = EndsAt;

        var response = await CreateAsAdminAsync(city.Id, ValidForm(PollType.ManagerVote, endsAt));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var stored = await QueryDbAsync(context => context.Polls.SingleAsync());
        Assert.Equal(city.Id, stored.CityId);
        Assert.Equal(PollTitle, stored.Title);
        Assert.Equal(PollDescription, stored.Description);
        Assert.Equal(PollType.ManagerVote, stored.Type);
        Assert.Equal(Budget, stored.BudgetAmount);
        Assert.Equal(PollStatus.Active, stored.Status);
        Assert.True((stored.EndsAt - endsAt).Duration() < DateTolerance);
        var returnedId = JsonDocument
            .Parse(await response.Content.ReadAsStringAsync())
            .RootElement
            .GetProperty(ValueProperty)
            .GetProperty(IdProperty)
            .GetGuid();
        Assert.Equal(stored.Id, returnedId);
    }

    [Fact]
    public async Task Handle_ValidRequest_SchedulesPollFinishOnce()
    {
        var city = await SeedCityAsync();
        var endsAt = EndsAt;

        await CreateAsAdminAsync(city.Id, ValidForm(endsAt: endsAt));

        var stored = await QueryDbAsync(context => context.Polls.SingleAsync());
        Factory.PollScheduler.Verify(
            x => x.ScheduleAsync(stored.Id, endsAt, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithImages_StoresThemInOrderAndUploadsOnce()
    {
        var city = await SeedCityAsync();

        var response = await CreateAsAdminAsync(
            city.Id,
            ValidForm(imageFileNames: [PollIntegrationHelpers.ImageFileName, SecondImageFileName]));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var images = await QueryDbAsync(context => context.PollImages.OrderBy(x => x.Order).ToListAsync());
        Assert.Equal([0, 1], images.Select(x => x.Order));
        Assert.All(images, image => Assert.StartsWith(EnvironmentConstants.ImageStorage.UploadedPrefix, image.FileName));
        VerifyUploadCalls(Times.Once());
    }

    [Fact]
    public async Task Handle_WithoutImages_StoresNoImagesAndDoesNotUpload()
    {
        var city = await SeedCityAsync();

        await CreateAsAdminAsync(city.Id, ValidForm());

        Assert.Empty(await QueryDbAsync(context => context.PollImages.ToListAsync()));
        VerifyUploadCalls(Times.Never());
    }

    [Fact]
    public async Task Handle_ActivePollOfSameTypeExists_ReturnsConflictAndStoresNothingNew()
    {
        var city = await SeedCityAsync();
        await SeedAsync(PollIntegrationHelpers.BuildPoll(city.Id, PollStatus.Active, PollType.UserVote));

        var response = await CreateAsAdminAsync(city.Id, ValidForm(PollType.UserVote));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(1, await QueryDbAsync(context => context.Polls.CountAsync()));
        Factory.PollScheduler.Verify(
            x => x.ScheduleAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ActivePollOfOtherTypeExists_StoresNewPoll()
    {
        var city = await SeedCityAsync();
        await SeedAsync(PollIntegrationHelpers.BuildPoll(city.Id, PollStatus.Active, PollType.UserVote));

        var response = await CreateAsAdminAsync(city.Id, ValidForm(PollType.ManagerVote));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, await QueryDbAsync(context => context.Polls.CountAsync()));
    }

    [Fact]
    public async Task Handle_PollOfSameTypeIsNotActive_StoresNewPoll()
    {
        var city = await SeedCityAsync();
        await SeedAsync(PollIntegrationHelpers.BuildPoll(city.Id, PollStatus.Suspended, PollType.UserVote));

        var response = await CreateAsAdminAsync(city.Id, ValidForm(PollType.UserVote));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, await QueryDbAsync(context => context.Polls.CountAsync()));
    }

    [Fact]
    public async Task Handle_ActivePollOfSameTypeExistsInAnotherCity_StoresNewPoll()
    {
        var city = await SeedCityAsync();
        var otherCity = await SeedCityAsync();
        await SeedAsync(PollIntegrationHelpers.BuildPoll(otherCity.Id, PollStatus.Active, PollType.UserVote));

        var response = await CreateAsAdminAsync(city.Id, ValidForm(PollType.UserVote));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, await QueryDbAsync(context => context.Polls.CountAsync()));
    }

    [Fact]
    public async Task Handle_CityDoesNotExist_ReturnsNotFound()
    {
        var response = await CreateAsAdminAsync(Guid.NewGuid(), ValidForm());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Empty(await QueryDbAsync(context => context.Polls.ToListAsync()));
    }

    [Theory]
    [MemberData(nameof(CreatePollIntegrationTestData.InvalidInputs), MemberType = typeof(CreatePollIntegrationTestData))]
    public async Task Handle_InputIsInvalid_ReturnsBadRequestAndStoresNothing(InvalidPollCase invalid)
    {
        var city = await SeedCityAsync();
        var form = PollIntegrationHelpers.BuildCreateForm(
            invalid.Title,
            invalid.Type,
            DateTimeOffset.UtcNow.AddDays(invalid.EndsInDays),
            invalid.Budget,
            invalid.Description);

        var response = await CreateAsAdminAsync(city.Id, form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(await QueryDbAsync(context => context.Polls.ToListAsync()));
    }

    [Fact]
    public async Task Handle_UserCreatesPollInOwnCity_StoresPoll()
    {
        var city = await SeedCityAsync();

        var response = await CreateAsUserAsync(city.Id, city.Id, ValidForm());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(city.Id, (await QueryDbAsync(context => context.Polls.SingleAsync())).CityId);
    }

    [Fact]
    public async Task Handle_UserCreatesPollInAnotherCity_ReturnsForbiddenAndStoresNothing()
    {
        var city = await SeedCityAsync();
        var userCity = await SeedCityAsync();

        var response = await CreateAsUserAsync(city.Id, userCity.Id, ValidForm());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Empty(await QueryDbAsync(context => context.Polls.ToListAsync()));
    }

    [Fact]
    public async Task Handle_AdminCreatesPollInAnyCity_IgnoresCityOfCaller()
    {
        var city = await SeedCityAsync();
        var client = CreateClient([Permissions.Polls.CreateAny], cityId: Guid.NewGuid());

        var response = await client.PostAsync(
            $"{PollIntegrationHelpers.AdminPollsRoute}/{PollIntegrationHelpers.CitiesSegment}/{city.Id}",
            ValidForm());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
