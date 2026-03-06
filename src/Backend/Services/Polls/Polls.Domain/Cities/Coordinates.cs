using Polls.Domain.Common;

namespace Polls.Domain.Cities;

public record Coordinates
{
    private Coordinates(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public double Latitude { get; }
    public double Longitude { get; }

    public static Result<Coordinates> Create(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
            return CityErrors.InvalidLatitude;

        if (longitude < -180 || longitude > 180)
            return CityErrors.InvalidLongitude;

        return new Coordinates(latitude, longitude);
    }
}
