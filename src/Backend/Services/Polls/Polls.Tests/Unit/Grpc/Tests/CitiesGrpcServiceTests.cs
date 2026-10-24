using AutoMapper;
using CityPulse.Contracts.Grpc.Protos;
using Grpc.Core;
using MediatR;
using Moq;
using Polls.API.Grpc;
using Polls.Application.Cities.DTOs;
using Polls.Application.Cities.Queries.GetCityById;
using Polls.Application.Common.Interfaces;
using Polls.Domain.Cities;
using Polls.Domain.Common;
using Polls.Tests.Infrastructure;
using Polls.Tests.Unit.Grpc.TestData;
using Xunit;
using DomainCityStatus = Polls.Domain.Cities.Enums.CityStatus;
using GrpcCityStatus = CityPulse.Contracts.Grpc.Protos.CityStatus;

namespace Polls.Tests.Unit.Grpc.Tests;

public sealed class CitiesGrpcServiceTests
{
    private const string InvalidIdMessage = "Invalid city id format";
    private const string CityTitle = "Warsaw";

    private readonly Mock<ISender> _sender = new(MockBehavior.Strict);
    private readonly IMapper _mapper = TestMapper.Create(Mock.Of<IImageStorageService>());
    private readonly CitiesGrpcService _sut;

    public CitiesGrpcServiceTests()
    {
        _sut = new CitiesGrpcService(_sender.Object, _mapper);
    }

    private static CityDto BuildCity(Guid id, int status = (int)DomainCityStatus.Active) => new()
    {
        Id = id,
        Title = CityTitle,
        Coordinates = new CoordinatesDto(52.23, 21.01),
        Status = status
    };

    private void SetupSender(Result<CityDto> result) =>
        _sender
            .Setup(x => x.Send(It.IsAny<GetCityByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

    [Theory]
    [MemberData(nameof(GrpcServiceTestData.InvalidIds), MemberType = typeof(GrpcServiceTestData))]
    public async Task GetCity_CityIdIsNotGuid_ThrowsInvalidArgumentWithoutQuerying(string cityId)
    {
        var request = new GetCityRequest { CityId = cityId };

        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _sut.GetCity(request, TestServerCallContext.Create()));

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
        Assert.Equal(InvalidIdMessage, exception.Status.Detail);
        _sender.Verify(
            x => x.Send(It.IsAny<GetCityByIdQuery>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetCity_CityIdIsValid_QueriesAllCitiesWithTokenFromContext()
    {
        var cityId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();
        SetupSender(BuildCity(cityId));

        await _sut.GetCity(new GetCityRequest { CityId = cityId.ToString() }, TestServerCallContext.Create(cts.Token));

        _sender.Verify(
            x => x.Send(
                It.Is<GetCityByIdQuery>(query => query.Id == cityId && !query.IncludeOnlyActive),
                cts.Token),
            Times.Once);
    }

    [Fact]
    public async Task GetCity_CityDoesNotExist_ThrowsNotFoundMentioningCityId()
    {
        var cityId = Guid.NewGuid();
        SetupSender(CityErrors.NotFound(cityId));

        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _sut.GetCity(new GetCityRequest { CityId = cityId.ToString() }, TestServerCallContext.Create()));

        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
        Assert.Contains(cityId.ToString(), exception.Status.Detail);
    }

    [Fact]
    public async Task GetCity_CityExists_ReturnsIdAndNameAsStrings()
    {
        var cityId = Guid.NewGuid();
        SetupSender(BuildCity(cityId));

        var response = await _sut.GetCity(new GetCityRequest { CityId = cityId.ToString() }, TestServerCallContext.Create());

        Assert.Equal(cityId.ToString(), response.Id);
        Assert.Equal(CityTitle, response.Name);
    }

    [Theory]
    [MemberData(nameof(GrpcServiceTestData.CityStatuses), MemberType = typeof(GrpcServiceTestData))]
    public async Task GetCity_CityExists_ReturnsMatchingStatus(DomainCityStatus status)
    {
        var cityId = Guid.NewGuid();
        SetupSender(BuildCity(cityId, (int)status));

        var response = await _sut.GetCity(new GetCityRequest { CityId = cityId.ToString() }, TestServerCallContext.Create());

        Assert.Equal((GrpcCityStatus)(int)status, response.Status);
    }
}
