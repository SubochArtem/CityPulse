using AutoMapper;
using CityPulse.Contracts.Grpc.Protos;
using Grpc.Core;
using MediatR;
using Moq;
using Polls.API.Grpc;
using Polls.Application.Common.Interfaces;
using Polls.Application.Ideas.DTOs;
using Polls.Application.Ideas.Queries.GetIdeaCity;
using Polls.Domain.Common;
using Polls.Domain.Ideas;
using Polls.Tests.Infrastructure;
using Polls.Tests.Unit.Grpc.TestData;
using Xunit;

namespace Polls.Tests.Unit.Grpc.Tests;

public sealed class IdeasGrpcServiceTests
{
    private const string InvalidIdMessage = "Invalid idea id format";

    private readonly Mock<ISender> _sender = new(MockBehavior.Strict);
    private readonly IMapper _mapper = TestMapper.Create(Mock.Of<IImageStorageService>());
    private readonly IdeasGrpcService _sut;

    public IdeasGrpcServiceTests()
    {
        _sut = new IdeasGrpcService(_sender.Object, _mapper);
    }

    private void SetupSender(Result<IdeaCityDto> result) =>
        _sender
            .Setup(x => x.Send(It.IsAny<GetIdeaCityQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

    [Theory]
    [MemberData(nameof(GrpcServiceTestData.InvalidIds), MemberType = typeof(GrpcServiceTestData))]
    public async Task GetIdeaCity_IdeaIdIsNotGuid_ThrowsInvalidArgumentWithoutQuerying(string ideaId)
    {
        var request = new GetIdeaCityRequest { IdeaId = ideaId };

        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _sut.GetIdeaCity(request, TestServerCallContext.Create()));

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
        Assert.Equal(InvalidIdMessage, exception.Status.Detail);
        _sender.Verify(
            x => x.Send(It.IsAny<GetIdeaCityQuery>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [MemberData(nameof(GrpcServiceTestData.IncludeOnlyActiveValues), MemberType = typeof(GrpcServiceTestData))]
    public async Task GetIdeaCity_IdeaIdIsValid_QueriesWithRequestedFlagAndTokenFromContext(bool includeOnlyActive)
    {
        var ideaId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();
        SetupSender(new IdeaCityDto { IdeaId = ideaId, CityId = Guid.NewGuid() });
        var request = new GetIdeaCityRequest { IdeaId = ideaId.ToString(), IncludeOnlyActive = includeOnlyActive };

        await _sut.GetIdeaCity(request, TestServerCallContext.Create(cts.Token));

        _sender.Verify(
            x => x.Send(
                It.Is<GetIdeaCityQuery>(query => query.Id == ideaId && query.IncludeOnlyActive == includeOnlyActive),
                cts.Token),
            Times.Once);
    }

    [Fact]
    public async Task GetIdeaCity_IdeaDoesNotExist_ThrowsNotFoundMentioningIdeaId()
    {
        var ideaId = Guid.NewGuid();
        SetupSender(IdeaErrors.NotFound(ideaId));

        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _sut.GetIdeaCity(new GetIdeaCityRequest { IdeaId = ideaId.ToString() }, TestServerCallContext.Create()));

        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
        Assert.Contains(ideaId.ToString(), exception.Status.Detail);
    }

    [Fact]
    public async Task GetIdeaCity_IdeaExists_ReturnsIdeaIdAndCityIdAsStrings()
    {
        var ideaId = Guid.NewGuid();
        var cityId = Guid.NewGuid();
        SetupSender(new IdeaCityDto { IdeaId = ideaId, CityId = cityId });

        var response = await _sut.GetIdeaCity(
            new GetIdeaCityRequest { IdeaId = ideaId.ToString() },
            TestServerCallContext.Create());

        Assert.Equal(ideaId.ToString(), response.IdeaId);
        Assert.Equal(cityId.ToString(), response.CityId);
    }
}
