using Xunit;

namespace Polls.Tests.Integration.Handlers.Cities.TestData;

public sealed record CoordinatesCase(double Latitude, double Longitude)
{
    public override string ToString() => $"({Latitude}, {Longitude})";
}

public static class CreateCityIntegrationTestData
{
    private const double JustAboveMaxLatitude = 90.1;
    private const double JustBelowMinLatitude = -90.1;
    private const double JustAboveMaxLongitude = 180.1;
    private const double JustBelowMinLongitude = -180.1;

    public static TheoryData<CoordinatesCase> InvalidCoordinates => new()
    {
        new CoordinatesCase(JustAboveMaxLatitude, 0),
        new CoordinatesCase(JustBelowMinLatitude, 0),
        new CoordinatesCase(0, JustAboveMaxLongitude),
        new CoordinatesCase(0, JustBelowMinLongitude)
    };
}
