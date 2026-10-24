using Polls.Domain.Cities.Enums;
using Xunit;

namespace Polls.Tests.Unit.Grpc.TestData;

public static class GrpcServiceTestData
{
    public static TheoryData<string> InvalidIds => new()
    {
        "",
        "   ",
        "not-a-guid",
        "12345",
        "00000000-0000-0000-0000"
    };

    public static TheoryData<CityStatus> CityStatuses => new()
    {
        CityStatus.Undefined,
        CityStatus.Active,
        CityStatus.Inactive
    };

    public static TheoryData<bool> IncludeOnlyActiveValues => new()
    {
        true,
        false
    };
}
