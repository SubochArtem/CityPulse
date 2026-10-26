using System.Net;
using System.Text.Json;
using Polls.Domain.Authorization;
using Polls.Domain.Cities.Enums;
using Polls.Tests.Infrastructure;
using Polls.Tests.Infrastructure.Constants;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Cities.Tests;

public sealed class GetCityByIdQueryHandlerTests(PollsWebAppFactory factory) : PollsApiTestBase(factory)
{
    private const string CityTitle = "Springfield";
    private const string CityDescription = "A city in the middle of nowhere";
    private const string FirstImageFileName = "first.jpg";
    private const string SecondImageFileName = "second.jpg";
    private const string IdProperty = "id";
    private const string CreatedAtProperty = "createdAt";
    private const string UpdatedAtProperty = "updatedAt";
    private const string TitleProperty = "title";
    private const string DescriptionProperty = "description";
    private const string StatusProperty = "status";
    private const string CoordinatesProperty = "coordinates";
    private const string LatitudeProperty = "latitude";
    private const string LongitudeProperty = "longitude";
    private const string ImagesProperty = "images";
    private const string UrlProperty = "url";

    private async Task<HttpResponseMessage> GetAsync(Guid id) =>
        await CreateClient([Permissions.Cities.ReadAny])
            .GetAsync($"{CityIntegrationHelpers.AdminCitiesRoute}/{id}");

    private static async Task<JsonElement> ReadValueAsync(HttpResponseMessage response) =>
        JsonDocument
            .Parse(await response.Content.ReadAsStringAsync())
            .RootElement;

    [Fact]
    public async Task Handle_CityExists_ReturnsItsData()
    {
        var city = CityIntegrationHelpers.BuildCity(CityTitle);
        city.Description = CityDescription;
        await SeedAsync(city);

        var response = await GetAsync(city.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var value = await ReadValueAsync(response);
        Assert.Equal(city.Id, value.GetProperty(IdProperty).GetGuid());
        Assert.NotEqual(default, value.GetProperty(CreatedAtProperty).GetDateTimeOffset());
        Assert.Equal(JsonValueKind.Null, value.GetProperty(UpdatedAtProperty).ValueKind);
        Assert.Equal(CityTitle, value.GetProperty(TitleProperty).GetString());
        Assert.Equal(CityDescription, value.GetProperty(DescriptionProperty).GetString());
        Assert.Equal((int)CityStatus.Active, value.GetProperty(StatusProperty).GetInt32());
        var coordinates = value.GetProperty(CoordinatesProperty);
        Assert.Equal(city.Coordinates.Latitude, coordinates.GetProperty(LatitudeProperty).GetDouble());
        Assert.Equal(city.Coordinates.Longitude, coordinates.GetProperty(LongitudeProperty).GetDouble());
    }

    [Fact]
    public async Task Handle_CityIsInactive_ReturnsItToAdmin()
    {
        var city = CityIntegrationHelpers.BuildCity(CityTitle, CityStatus.Inactive);
        await SeedAsync(city);

        var response = await GetAsync(city.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var value = await ReadValueAsync(response);
        Assert.Equal((int)CityStatus.Inactive, value.GetProperty(StatusProperty).GetInt32());
    }

    [Fact]
    public async Task Handle_CityHasNoImages_ReturnsEmptyImageList()
    {
        var city = CityIntegrationHelpers.BuildCity(CityTitle);
        await SeedAsync(city);

        var value = await ReadValueAsync(await GetAsync(city.Id));

        Assert.Equal(0, value.GetProperty(ImagesProperty).GetArrayLength());
    }

    [Fact]
    public async Task Handle_CityHasImages_ReturnsImageUrlsFromStorage()
    {
        var city = CityIntegrationHelpers.BuildCity(
            CityTitle,
            CityStatus.Active,
            FirstImageFileName,
            SecondImageFileName);
        await SeedAsync(city);

        var response = await GetAsync(city.Id);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var images = (await ReadValueAsync(response)).GetProperty(ImagesProperty);
        Assert.Equal(
            new[]
            {
                EnvironmentConstants.ImageStorage.UrlPrefix + FirstImageFileName,
                EnvironmentConstants.ImageStorage.UrlPrefix + SecondImageFileName
            },
            images.EnumerateArray().Select(image => image.GetProperty(UrlProperty).GetString()).ToArray());
    }

    [Fact]
    public async Task Handle_CityDoesNotExist_ReturnsNotFound()
    {
        var response = await GetAsync(Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
