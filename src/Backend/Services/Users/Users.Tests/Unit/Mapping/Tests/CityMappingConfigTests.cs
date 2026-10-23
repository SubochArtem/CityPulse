using CityPulse.Contracts.Grpc.Protos;
using Mapster;
using Users.DataAccess.DTOs;
using Users.DataAccess.Mapping;
using Users.Tests.Unit.Mapping.TestData;
using Xunit;
using DtoCityStatus = Users.DataAccess.DTOs.CityStatus;
using GrpcCityStatus = CityPulse.Contracts.Grpc.Protos.CityStatus;

namespace Users.Tests.Unit.Mapping.Tests;

public sealed class CityMappingConfigTests
{
    private const string CityName = "Warsaw";

    private static TypeAdapterConfig BuildIsolatedConfig()
    {
        var config = new TypeAdapterConfig();
        new CityMappingConfig().Register(config);
        return config;
    }

    private static GetCityResponse BuildResponse(
        string id,
        GrpcCityStatus status = GrpcCityStatus.Active) => new()
    {
        Id = id,
        Name = CityName,
        Status = status
    };

    [Fact]
    public void Map_GetCityResponseToCityDto_MapsIdAndName()
    {
        var config = BuildIsolatedConfig();
        var cityId = Guid.NewGuid();

        var dto = BuildResponse(cityId.ToString()).Adapt<CityDto>(config);

        Assert.Equal(cityId, dto.Id);
        Assert.Equal(CityName, dto.Name);
    }

    [Theory]
    [MemberData(nameof(CityMappingConfigTestData.StatusMappings), MemberType = typeof(CityMappingConfigTestData))]
    public void Map_GetCityResponseToCityDto_MapsStatus(GrpcCityStatus grpcStatus, DtoCityStatus expectedStatus)
    {
        var config = BuildIsolatedConfig();

        var dto = BuildResponse(Guid.NewGuid().ToString(), grpcStatus).Adapt<CityDto>(config);

        Assert.Equal(expectedStatus, dto.Status);
    }

    [Theory]
    [MemberData(nameof(CityMappingConfigTestData.UnknownStatuses), MemberType = typeof(CityMappingConfigTestData))]
    public void Map_GetCityResponseToCityDto_UnknownStatus_ThrowsArgumentOutOfRangeException(GrpcCityStatus unknownStatus)
    {
        var config = BuildIsolatedConfig();
        var response = BuildResponse(Guid.NewGuid().ToString(), unknownStatus);

        Assert.Throws<ArgumentOutOfRangeException>(() => response.Adapt<CityDto>(config));
    }

    [Theory]
    [MemberData(nameof(CityMappingConfigTestData.InvalidCityIds), MemberType = typeof(CityMappingConfigTestData))]
    public void Map_GetCityResponseToCityDto_InvalidId_ThrowsFormatException(string invalidId)
    {
        var config = BuildIsolatedConfig();
        var response = BuildResponse(invalidId);

        Assert.Throws<FormatException>(() => response.Adapt<CityDto>(config));
    }
}
