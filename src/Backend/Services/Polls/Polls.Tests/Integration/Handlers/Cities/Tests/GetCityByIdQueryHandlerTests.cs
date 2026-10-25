using System.Net;
using System.Text.Json;
using Polls.Domain.Authorization;
using Polls.Tests.Infrastructure;
using Polls.Tests.Infrastructure.Constants;
using Polls.Tests.TestData;
using Xunit;

namespace Polls.Tests.Integration.Handlers.Cities.Tests;

public sealed class GetCityByIdQueryHandlerTests(PollsWebAppFactory factory) : PollsApiTestBase(factory)
{
    private const string AdminCitiesRoute = "api/v1/admin/cities";
    private const string FirstImageFileName = "first.jpg";
    private const string SecondImageFileName = "second.jpg";
    private const string ValueProperty = "value";
    private const string ImagesProperty = "images";
    private const string UrlProperty = "url";

    [Fact]
    public async Task Handle_CityHasImages_ReturnsImageUrlsFromStorage()
    {
        var city = CityFakers.CityWithImages(FirstImageFileName, SecondImageFileName);
        await SeedAsync(city);

        var response = await CreateClient([Permissions.Cities.ReadAny])
            .GetAsync($"{AdminCitiesRoute}/{city.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var images = JsonDocument
            .Parse(await response.Content.ReadAsStringAsync())
            .RootElement
            .GetProperty(ValueProperty)
            .GetProperty(ImagesProperty);
        Assert.Equal(
            [
                EnvironmentConstants.ImageStorage.UrlPrefix + FirstImageFileName,
                EnvironmentConstants.ImageStorage.UrlPrefix + SecondImageFileName
            ],
            images.EnumerateArray().Select(image => image.GetProperty(UrlProperty).GetString()));
    }
}
