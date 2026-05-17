using AutoMapper;
using Polls.Application.Images.DTOs;
using Polls.Application.Images.Resolvers;
using Polls.Application.Polls.DTOs;
using Polls.Domain.Images;
using Polls.Domain.Polls;

namespace Polls.Application.Polls.Mappings;

public sealed class PollMappingProfile : Profile
{
    public PollMappingProfile()
    {
        CreateMap<PollImage, ImageDto>()
            .ForMember(d => d.Url, opt => opt.MapFrom<ImageUrlResolver>());
        CreateMap<Poll, PollDto>()
            .ForMember(d => d.Images, opt => opt.MapFrom(src => src.Images));
        CreateMap<Poll, PollWithIdeasDto>()
            .ForMember(dest => dest.Ideas, opt => opt.MapFrom(src => src.Ideas))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images));
    }
}
