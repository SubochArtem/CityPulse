using Polls.Domain.Cities.Enums;
using Xunit;

namespace Polls.Tests.Unit.Validators.TestData;

public static class ChangeCityStatusCommandValidatorTestData
{
    public static TheoryData<CityStatus> UnknownStatuses => new()
    {
        (CityStatus)99,
        (CityStatus)(-1)
    };

    public static TheoryData<CityStatus> DefinedStatuses => new()
    {
        CityStatus.Undefined,
        CityStatus.Active,
        CityStatus.Inactive
    };
}
