using AutoMapper;
using CityPulse.Contracts.Grpc.Protos;
using Moq;
using Polls.Application.Common.Interfaces;
using Polls.Application.Ideas.DTOs;
using Polls.Tests.Infrastructure;
using Xunit;

namespace Polls.Tests.Unit.Mapping.Tests;

public sealed class IdeaGrpcProfileTests
{
    private readonly IMapper _mapper = TestMapper.Create(Mock.Of<IImageStorageService>());

    [Fact]
    public void Map_IdeaCityDtoToGetIdeaCityResponse_MapsBothIdsAsStrings()
    {
        var dto = new IdeaCityDto { IdeaId = Guid.NewGuid(), CityId = Guid.NewGuid() };

        var response = _mapper.Map<GetIdeaCityResponse>(dto);

        Assert.Equal(dto.IdeaId.ToString(), response.IdeaId);
        Assert.Equal(dto.CityId.ToString(), response.CityId);
    }

    [Fact]
    public void Map_IdeaCityDtoWithEmptyIds_MapsThemToEmptyGuidStrings()
    {
        var dto = new IdeaCityDto { IdeaId = Guid.Empty, CityId = Guid.Empty };

        var response = _mapper.Map<GetIdeaCityResponse>(dto);

        Assert.Equal(Guid.Empty.ToString(), response.IdeaId);
        Assert.Equal(Guid.Empty.ToString(), response.CityId);
    }
}
