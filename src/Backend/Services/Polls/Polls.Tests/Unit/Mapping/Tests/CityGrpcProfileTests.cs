using AutoMapper;
using CityPulse.Contracts.Grpc.Protos;
using Moq;
using Polls.Application.Cities.DTOs;
using Polls.Application.Common.Interfaces;
using Polls.Tests.Infrastructure;
using Polls.Tests.Unit.Grpc.TestData;
using Xunit;
using DomainCityStatus = Polls.Domain.Cities.Enums.CityStatus;
using GrpcCityStatus = CityPulse.Contracts.Grpc.Protos.CityStatus;

namespace Polls.Tests.Unit.Mapping.Tests;

public sealed class CityGrpcProfileTests
{
    private const string CityTitle = "Warsaw";
    private const int UnknownStatus = 99;

    private readonly IMapper _mapper = TestMapper.Create(Mock.Of<IImageStorageService>());

    private static CityDto BuildCity(int status) => new()
    {
        Id = Guid.NewGuid(),
        Title = CityTitle,
        Coordinates = new CoordinatesDto(52.23, 21.01),
        Status = status
    };

    [Fact]
    public void Map_CityDtoToGetCityResponse_MapsIdAsStringAndTitleAsName()
    {
        var city = BuildCity((int)DomainCityStatus.Active);

        var response = _mapper.Map<GetCityResponse>(city);

        Assert.Equal(city.Id.ToString(), response.Id);
        Assert.Equal(CityTitle, response.Name);
    }

    [Theory]
    [MemberData(nameof(GrpcServiceTestData.CityStatuses), MemberType = typeof(GrpcServiceTestData))]
    public void Map_CityDtoToGetCityResponse_MapsStatus(DomainCityStatus status)
    {
        var response = _mapper.Map<GetCityResponse>(BuildCity((int)status));

        Assert.Equal((GrpcCityStatus)(int)status, response.Status);
    }

    [Theory]
    [InlineData(DomainCityStatus.Undefined, GrpcCityStatus.Undefined)]
    [InlineData(DomainCityStatus.Active, GrpcCityStatus.Active)]
    [InlineData(DomainCityStatus.Inactive, GrpcCityStatus.Inactive)]
    public void Map_DomainStatusToGrpcStatus_MapsEachKnownValue(DomainCityStatus domain, GrpcCityStatus expected)
    {
        var result = _mapper.Map<GrpcCityStatus>(domain);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Map_UnknownDomainStatusToGrpcStatus_ThrowsArgumentOutOfRange()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => _mapper.Map<GrpcCityStatus>((DomainCityStatus)UnknownStatus));

        Assert.Equal("status", exception.ParamName);
    }
}
