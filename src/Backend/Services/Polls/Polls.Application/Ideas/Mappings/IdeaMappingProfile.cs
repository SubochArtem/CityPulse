using AutoMapper;
using Polls.Application.Ideas.DTOs;
using Polls.Application.Images.DTOs;
using Polls.Application.Images.Resolvers;
using Polls.Domain.Ideas;
using Polls.Domain.Images;

namespace Polls.Application.Ideas.Mappings;

public sealed class IdeaMappingProfile : Profile
{
    public IdeaMappingProfile()
    {
        CreateMap<IdeaImage, ImageDto>()
            .ForMember(d => d.Url, opt => opt.MapFrom<ImageUrlResolver>());
        CreateMap<Idea, IdeaDto>()
            .ForMember(d => d.Images, opt => opt.MapFrom(src => src.Images));
        CreateMap<Idea, IdeaWithPollDto>()
            .ForMember(dest => dest.Poll, opt => opt.MapFrom(src => src.Poll))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images));
    }
}
