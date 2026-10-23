using AutoMapper;
using CityPulse.Contracts.Grpc.Protos;
using Polls.Application.Ideas.DTOs;

namespace Polls.API.Mapping;

public class IdeaGrpcProfile : Profile
{
    public IdeaGrpcProfile()
    {
        CreateMap<IdeaCityDto, GetIdeaCityResponse>()
            .ForMember(dest => dest.IdeaId, opt => opt.MapFrom(src => src.IdeaId.ToString()))
            .ForMember(dest => dest.CityId, opt => opt.MapFrom(src => src.CityId.ToString()));
    }
}
