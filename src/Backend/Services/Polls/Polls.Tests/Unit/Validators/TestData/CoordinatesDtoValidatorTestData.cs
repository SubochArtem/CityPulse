using Xunit;

namespace Polls.Tests.Unit.Validators.TestData;

public static class CoordinatesDtoValidatorTestData
{
    public static TheoryData<double> InvalidLatitudes => new()
    {
        -90.0001,
        90.0001,
        -91,
        91,
        double.NaN,
        double.NegativeInfinity,
        double.PositiveInfinity
    };

    public static TheoryData<double> ValidLatitudes => new()
    {
        -90,
        0,
        55.75,
        90
    };

    public static TheoryData<double> InvalidLongitudes => new()
    {
        -180.0001,
        180.0001,
        -181,
        181,
        double.NaN,
        double.NegativeInfinity,
        double.PositiveInfinity
    };

    public static TheoryData<double> ValidLongitudes => new()
    {
        -180,
        0,
        37.62,
        180
    };
}
