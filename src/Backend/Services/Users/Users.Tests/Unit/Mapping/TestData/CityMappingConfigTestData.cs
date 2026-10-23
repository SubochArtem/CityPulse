using Xunit;
using DtoCityStatus = Users.DataAccess.DTOs.CityStatus;
using GrpcCityStatus = CityPulse.Contracts.Grpc.Protos.CityStatus;

namespace Users.Tests.Unit.Mapping.TestData;

public static class CityMappingConfigTestData
{
    public static TheoryData<GrpcCityStatus, DtoCityStatus> StatusMappings => new()
    {
        { GrpcCityStatus.Undefined, DtoCityStatus.Undefined },
        { GrpcCityStatus.Active, DtoCityStatus.Active },
        { GrpcCityStatus.Inactive, DtoCityStatus.Inactive }
    };

    public static TheoryData<GrpcCityStatus> UnknownStatuses => new()
    {
        (GrpcCityStatus)3,
        (GrpcCityStatus)99,
        (GrpcCityStatus)(-1)
    };

    public static TheoryData<string> InvalidCityIds => new()
    {
        "",
        "not-a-guid",
        "12345"
    };
}
