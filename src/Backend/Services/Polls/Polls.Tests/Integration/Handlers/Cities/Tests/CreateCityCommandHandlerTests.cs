using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Moq;
using Polls.Application.Common.Constants;
using Polls.Application.Common.Models;
using Polls.Domain.Authorization;
using Polls.Domain.Cities.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.Infrastructure.Constants;
using Polls.Tests.Integration.Handlers.Cities.TestData;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Cities.Tests;

public sealed class CreateCityCommandHandlerTests(PollsWebAppFactory factory) : PollsApiTestBase(factory)
{
    private const string CityTitle = "Springfield";
    private const string CityDescription = "A city in the middle of nowhere";
    private const string SecondImageFileName = "second.jpg";
    private const double Latitude = 48.5;
    private const double Longitude = -12.25;
    private const string ValueProperty = "value";
    private const string IdProperty = "id";

    private Task<HttpResponseMessage> CreateAsync(MultipartFormDataContent form) =>
        CreateClient([Permissions.Cities.CreateAny])
            .PostAsync(CityIntegrationHelpers.AdminCitiesRoute, form);

    private static Guid ReadCreatedId(string body) =>
        JsonDocument.Parse(body).RootElement.GetProperty(ValueProperty).GetProperty(IdProperty).GetGuid();

    private void VerifyUploadCalls(Times times) =>
        Factory.ImageStorage.Verify(
            x => x.UploadRangeAsync(It.IsAny<IReadOnlyList<ImageFile>>(), It.IsAny<CancellationToken>()),
            times);

    [Fact]
    public async Task Handle_ValidRequest_StoresActiveCityWithGivenData()
    {
        var response = await CreateAsync(CityIntegrationHelpers.BuildCreateForm(
            CityTitle,
            Latitude,
            Longitude,
            CityDescription));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var stored = await QueryDbAsync(context => context.Cities.SingleAsync());
        Assert.Equal(CityTitle, stored.Title);
        Assert.Equal(CityDescription, stored.Description);
        Assert.Equal(Latitude, stored.Coordinates.Latitude);
        Assert.Equal(Longitude, stored.Coordinates.Longitude);
        Assert.Equal(CityStatus.Active, stored.Status);
        Assert.Equal(stored.Id, ReadCreatedId(await response.Content.ReadAsStringAsync()));
    }

    [Fact]
    public async Task Handle_NoDescription_StoresNullDescription()
    {
        await CreateAsync(CityIntegrationHelpers.BuildCreateForm(CityTitle, Latitude, Longitude));

        var stored = await QueryDbAsync(context => context.Cities.SingleAsync());
        Assert.Null(stored.Description);
    }

    [Fact]
    public async Task Handle_WithImages_StoresThemInOrderAndUploadsOnce()
    {
        var response = await CreateAsync(CityIntegrationHelpers.BuildCreateForm(
            CityTitle,
            Latitude,
            Longitude,
            imageFileNames: [CityIntegrationHelpers.ImageFileName, SecondImageFileName]));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var images = await QueryDbAsync(context => context.CityImages.OrderBy(x => x.Order).ToListAsync());
        Assert.Equal([0, 1], images.Select(x => x.Order));
        Assert.All(images, image => Assert.StartsWith(EnvironmentConstants.ImageStorage.UploadedPrefix, image.FileName));
        VerifyUploadCalls(Times.Once());
    }

    [Fact]
    public async Task Handle_WithoutImages_StoresNoImagesAndDoesNotUpload()
    {
        await CreateAsync(CityIntegrationHelpers.BuildCreateForm(CityTitle, Latitude, Longitude));

        Assert.Empty(await QueryDbAsync(context => context.CityImages.ToListAsync()));
        VerifyUploadCalls(Times.Never());
    }

    [Fact]
    public async Task Handle_TitleAlreadyExists_ReturnsConflictAndKeepsSingleCity()
    {
        await SeedAsync(CityIntegrationHelpers.BuildCity(CityTitle));

        var response = await CreateAsync(CityIntegrationHelpers.BuildCreateForm(CityTitle, Latitude, Longitude));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(1, await QueryDbAsync(context => context.Cities.CountAsync()));
    }

    [Fact]
    public async Task Handle_TitleIsMissing_ReturnsBadRequestAndStoresNothing()
    {
        var response = await CreateAsync(CityIntegrationHelpers.BuildCreateForm(string.Empty, Latitude, Longitude));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(await QueryDbAsync(context => context.Cities.ToListAsync()));
    }

    [Fact]
    public async Task Handle_TitleIsTooLong_ReturnsBadRequestAndStoresNothing()
    {
        var title = new string('a', ValidationConstants.City.MaxTitleLength + 1);

        var response = await CreateAsync(CityIntegrationHelpers.BuildCreateForm(title, Latitude, Longitude));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(await QueryDbAsync(context => context.Cities.ToListAsync()));
    }

    [Theory]
    [MemberData(nameof(CreateCityIntegrationTestData.InvalidCoordinates), MemberType = typeof(CreateCityIntegrationTestData))]
    public async Task Handle_CoordinatesAreOutOfRange_ReturnsBadRequestAndStoresNothing(CoordinatesCase coordinates)
    {
        var response = await CreateAsync(CityIntegrationHelpers.BuildCreateForm(
            CityTitle,
            coordinates.Latitude,
            coordinates.Longitude));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(await QueryDbAsync(context => context.Cities.ToListAsync()));
    }
}
