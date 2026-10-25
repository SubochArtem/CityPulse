using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Moq;
using Polls.Application.Common.Constants;
using Polls.Application.Common.Models;
using Polls.Domain.Authorization;
using Polls.Domain.Cities;
using Polls.Domain.Cities.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.Infrastructure.Constants;
using Polls.Tests.Integration.Handlers.Cities.TestData;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Cities.Tests;

public sealed class UpdateCityCommandHandlerTests(PollsWebAppFactory factory) : PollsApiTestBase(factory)
{
    private const string OriginalTitle = "Springfield";
    private const string OtherTitle = "Shelbyville";
    private const string NewTitle = "Capital City";
    private const string NewDescription = "Known for its Monorail";
    private const string FirstImageFileName = "first.jpg";
    private const string SecondImageFileName = "second.jpg";
    private const string AddedImageFileName = "added.jpg";
    private const double NewLatitude = 12.5;
    private const double NewLongitude = -45.75;
    private const string ValueProperty = "value";
    private const string TitleProperty = "title";

    private Task<HttpResponseMessage> UpdateAsync(Guid id, MultipartFormDataContent form) =>
        CreateClient([Permissions.Cities.UpdateAny])
            .PutAsync($"{CityIntegrationHelpers.AdminCitiesRoute}/{id}", form);

    private Task<City> LoadCityAsync(Guid id) =>
        QueryDbAsync(context => context.Cities.Include(x => x.Images).SingleAsync(x => x.Id == id));

    private async Task<City> SeedCityAsync(
        string title = OriginalTitle,
        CityStatus status = CityStatus.Active,
        params string[] imageFileNames)
    {
        var city = CityIntegrationHelpers.BuildCity(title, status, imageFileNames);
        await SeedAsync(city);

        return city;
    }

    private void VerifyUploadCalls(Times times) =>
        Factory.ImageStorage.Verify(
            x => x.UploadRangeAsync(It.IsAny<IReadOnlyList<ImageFile>>(), It.IsAny<CancellationToken>()),
            times);

    [Fact]
    public async Task Handle_ValidRequest_UpdatesFieldsAndReturnsThem()
    {
        var city = await SeedCityAsync();

        var response = await UpdateAsync(city.Id, CityIntegrationHelpers.BuildUpdateForm(
            NewTitle,
            NewLatitude,
            NewLongitude,
            NewDescription));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var stored = await LoadCityAsync(city.Id);
        Assert.Equal(NewTitle, stored.Title);
        Assert.Equal(NewDescription, stored.Description);
        Assert.Equal(NewLatitude, stored.Coordinates.Latitude);
        Assert.Equal(NewLongitude, stored.Coordinates.Longitude);
        var returnedTitle = JsonDocument
            .Parse(await response.Content.ReadAsStringAsync())
            .RootElement
            .GetProperty(ValueProperty)
            .GetProperty(TitleProperty)
            .GetString();
        Assert.Equal(NewTitle, returnedTitle);
    }

    [Fact]
    public async Task Handle_InactiveCity_KeepsItsStatus()
    {
        var city = await SeedCityAsync(status: CityStatus.Inactive);

        await UpdateAsync(city.Id, CityIntegrationHelpers.BuildUpdateForm(NewTitle, NewLatitude, NewLongitude));

        Assert.Equal(CityStatus.Inactive, (await LoadCityAsync(city.Id)).Status);
    }

    [Fact]
    public async Task Handle_ImagesToAdd_UploadsThemOnceAndStoresAfterExistingOnes()
    {
        var city = await SeedCityAsync(OriginalTitle, CityStatus.Active, FirstImageFileName);

        var response = await UpdateAsync(city.Id, CityIntegrationHelpers.BuildUpdateForm(
            OriginalTitle,
            NewLatitude,
            NewLongitude,
            imageFileNamesToAdd: [AddedImageFileName, SecondImageFileName]));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var images = (await LoadCityAsync(city.Id)).Images.OrderBy(x => x.Order).ToList();
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
        var city = await SeedCityAsync(OriginalTitle, CityStatus.Active, FirstImageFileName, SecondImageFileName);
        var removed = city.Images.Single(x => x.FileName == FirstImageFileName);

        var response = await UpdateAsync(city.Id, CityIntegrationHelpers.BuildUpdateForm(
            OriginalTitle,
            NewLatitude,
            NewLongitude,
            imageIdsToDelete: [removed.Id]));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var images = (await LoadCityAsync(city.Id)).Images;
        Assert.Equal([SecondImageFileName], images.Select(x => x.FileName));
        var deleted = await QueryDbAsync(context => context.DeletedImages.ToListAsync());
        Assert.Equal([FirstImageFileName], deleted.Select(x => x.FileName));
        VerifyUploadCalls(Times.Never());
    }

    [Fact]
    public async Task Handle_ImageBelongsToAnotherCity_ReturnsConflictAndChangesNothing()
    {
        var city = await SeedCityAsync(OriginalTitle, CityStatus.Active, FirstImageFileName);
        var otherCity = CityIntegrationHelpers.BuildCity(OtherTitle, CityStatus.Active, SecondImageFileName);
        await SeedAsync(otherCity);

        var response = await UpdateAsync(city.Id, CityIntegrationHelpers.BuildUpdateForm(
            NewTitle,
            NewLatitude,
            NewLongitude,
            imageIdsToDelete: [otherCity.Images.Single().Id]));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(OriginalTitle, (await LoadCityAsync(city.Id)).Title);
        Assert.Single((await LoadCityAsync(otherCity.Id)).Images);
        Assert.Empty(await QueryDbAsync(context => context.DeletedImages.ToListAsync()));
    }

    [Fact]
    public async Task Handle_TitleIsUnchanged_ReturnsOk()
    {
        var city = await SeedCityAsync();

        var response = await UpdateAsync(city.Id, CityIntegrationHelpers.BuildUpdateForm(
            OriginalTitle,
            NewLatitude,
            NewLongitude,
            NewDescription));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(NewDescription, (await LoadCityAsync(city.Id)).Description);
    }

    [Fact]
    public async Task Handle_TitleBelongsToAnotherCity_ReturnsConflictAndChangesNothing()
    {
        var city = await SeedCityAsync();
        await SeedAsync(CityIntegrationHelpers.BuildCity(OtherTitle));

        var response = await UpdateAsync(city.Id, CityIntegrationHelpers.BuildUpdateForm(
            OtherTitle,
            NewLatitude,
            NewLongitude));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(OriginalTitle, (await LoadCityAsync(city.Id)).Title);
    }

    [Fact]
    public async Task Handle_CityDoesNotExist_ReturnsNotFound()
    {
        var response = await UpdateAsync(
            Guid.NewGuid(),
            CityIntegrationHelpers.BuildUpdateForm(NewTitle, NewLatitude, NewLongitude));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Handle_TitleIsMissing_ReturnsBadRequestAndChangesNothing()
    {
        var city = await SeedCityAsync();

        var response = await UpdateAsync(
            city.Id,
            CityIntegrationHelpers.BuildUpdateForm(string.Empty, NewLatitude, NewLongitude));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(OriginalTitle, (await LoadCityAsync(city.Id)).Title);
    }

    [Fact]
    public async Task Handle_TitleIsTooLong_ReturnsBadRequestAndChangesNothing()
    {
        var city = await SeedCityAsync();
        var title = new string('a', ValidationConstants.City.MaxTitleLength + 1);

        var response = await UpdateAsync(
            city.Id,
            CityIntegrationHelpers.BuildUpdateForm(title, NewLatitude, NewLongitude));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(OriginalTitle, (await LoadCityAsync(city.Id)).Title);
    }

    [Theory]
    [MemberData(nameof(CreateCityIntegrationTestData.InvalidCoordinates), MemberType = typeof(CreateCityIntegrationTestData))]
    public async Task Handle_CoordinatesAreOutOfRange_ReturnsBadRequestAndChangesNothing(CoordinatesCase coordinates)
    {
        var city = await SeedCityAsync();

        var response = await UpdateAsync(city.Id, CityIntegrationHelpers.BuildUpdateForm(
            NewTitle,
            coordinates.Latitude,
            coordinates.Longitude));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(OriginalTitle, (await LoadCityAsync(city.Id)).Title);
    }
}
