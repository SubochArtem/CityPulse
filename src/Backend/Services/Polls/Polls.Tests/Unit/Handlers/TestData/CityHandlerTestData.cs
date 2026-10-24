using Polls.Domain.Cities.Enums;
using Xunit;

namespace Polls.Tests.Unit.Handlers.TestData;

public static class CityHandlerTestData
{
    public static TheoryData<CityStatus> NotActiveStatuses => new()
    {
        CityStatus.Undefined,
        CityStatus.Inactive
    };

    public static TheoryData<CityStatus?> AnyFilterStatuses => new()
    {
        null,
        CityStatus.Undefined,
        CityStatus.Active,
        CityStatus.Inactive
    };

    public static TheoryData<CityStatus?> NonActiveFilterStatuses => new()
    {
        null,
        CityStatus.Undefined,
        CityStatus.Inactive
    };
}
