using AutoMapper;
using Moq;
using Polls.Application.Cities.DTOs;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Cities;
using Polls.Domain.Images;
using Polls.Tests.Infrastructure;
using Polls.Tests.TestData;
using Xunit;

namespace Polls.Tests.Unit.Mapping.Tests;

public sealed class CityMappingProfileTests
{
    private const string UrlPrefix = "https://storage.example.com/";

    private readonly Mock<IImageStorageService> _storage = new(MockBehavior.Strict);
    private readonly IMapper _mapper;

    public CityMappingProfileTests()
    {
        _storage
            .Setup(x => x.GetUrl(It.IsAny<string>()))
            .Returns<string>(fileName => UrlPrefix + fileName);

        _mapper = TestMapper.Create(_storage.Object);
    }

    private static City BuildCityWithImages()
    {
        var city = CityFakers.Cities().Generate();
        city.Images =
        [
            new CityImage { Id = Guid.NewGuid(), FileName = "first.jpg", Order = 0, CityId = city.Id },
            new CityImage { Id = Guid.NewGuid(), FileName = "second.png", Order = 1, CityId = city.Id }
        ];

        return city;
    }

    [Fact]
    public void Map_CityToCityDto_MapsScalarFields()
    {
        var city = CityFakers.Cities().Generate();

        var dto = _mapper.Map<CityDto>(city);

        Assert.Equal(city.Id, dto.Id);
        Assert.Equal(city.Title, dto.Title);
        Assert.Equal(city.Description, dto.Description);
        Assert.Equal((int)city.Status, dto.Status);
    }

    [Fact]
    public void Map_CityToCityDto_MapsCoordinates()
    {
        var city = CityFakers.Cities().Generate();

        var dto = _mapper.Map<CityDto>(city);

        Assert.Equal(city.Coordinates.Latitude, dto.Coordinates.Latitude);
        Assert.Equal(city.Coordinates.Longitude, dto.Coordinates.Longitude);
    }

    [Fact]
    public void Map_CityToCityDto_MapsImagesWithUrlsFromStorage()
    {
        var city = BuildCityWithImages();

        var dto = _mapper.Map<CityDto>(city);

        Assert.Equal(city.Images.Select(i => i.Id), dto.Images.Select(i => i.Id));
        Assert.Equal(city.Images.Select(i => i.Order), dto.Images.Select(i => i.Order));
        Assert.Equal(city.Images.Select(i => UrlPrefix + i.FileName), dto.Images.Select(i => i.Url));
    }

    [Fact]
    public void Map_CityWithoutImagesToCityDto_ReturnsEmptyImages()
    {
        var city = CityFakers.Cities().Generate();

        var dto = _mapper.Map<CityDto>(city);

        Assert.Empty(dto.Images);
    }

    [Fact]
    public void Map_CityToCityWithPollsDto_MapsPollsAndImages()
    {
        var city = BuildCityWithImages();
        city.Polls = PollFakers.Polls().Generate(2);

        var dto = _mapper.Map<CityWithPollsDto>(city);

        Assert.Equal(city.Id, dto.Id);
        Assert.Equal(city.Title, dto.Title);
        Assert.Equal(city.Polls.Select(p => p.Id), dto.Polls.Select(p => p.Id));
        Assert.Equal(city.Images.Select(i => UrlPrefix + i.FileName), dto.Images.Select(i => i.Url));
    }
}
