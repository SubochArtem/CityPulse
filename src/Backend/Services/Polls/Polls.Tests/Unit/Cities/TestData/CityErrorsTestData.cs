using Polls.Domain.Cities.Enums;
using Xunit;

namespace Polls.Tests.Unit.Cities.TestData;

public static class CityErrorsTestData
{
    public static TheoryData<CityStatus> AllStatuses => new()
    {
        CityStatus.Undefined,
        CityStatus.Active,
        CityStatus.Inactive
    };
}
