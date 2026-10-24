using Bogus;
using Polls.Domain.Cities;
using Polls.Domain.Cities.Enums;
using Polls.Domain.Images;

namespace Polls.Tests.TestData;

internal static class CityFakers
{
    private const int Seed = 20261024;
    private const string DefaultLocale = "en";

    public static Faker<City> Cities() =>
        new Faker<City>(DefaultLocale)
            .UseSeed(Seed)
            .StrictMode(true)
            .RuleFor(x => x.Id, f => f.Random.Guid())
            .RuleFor(x => x.CreatedAt, f => f.Date.PastOffset().ToUniversalTime())
            .RuleFor(x => x.UpdatedAt, _ => null)
            .RuleFor(x => x.Title, f => f.Address.City())
            .RuleFor(x => x.Description, f => f.Lorem.Sentence())
            .RuleFor(x => x.Coordinates, f => new Coordinates(f.Address.Latitude(), f.Address.Longitude()))
            .RuleFor(x => x.Status, _ => CityStatus.Active)
            .Ignore(x => x.Polls)
            .Ignore(x => x.Images);

    public static City CityWithImages(params string[] fileNames)
    {
        var city = Cities().Generate();
        city.Images = fileNames
            .Select((fileName, order) => new CityImage
            {
                Id = Guid.NewGuid(),
                FileName = fileName,
                Order = order,
                CityId = city.Id
            })
            .ToList();

        return city;
    }
}
