using AutoMapper;
using Polls.Application.Polls.DTOs;
using Polls.Domain.Polls;

namespace Polls.Application.Polls.Mappings;

public sealed class PollMappingProfile : Profile
{
    public PollMappingProfile()
    {
        CreateMap<Poll, PollDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<Poll, PollWithIdeasDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Ideas, opt => opt.MapFrom(src => src.Ideas));
    }
}
