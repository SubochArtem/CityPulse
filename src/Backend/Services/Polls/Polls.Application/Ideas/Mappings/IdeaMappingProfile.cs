using AutoMapper;
using Polls.Application.Ideas.DTOs;
using Polls.Domain.Ideas;

namespace Polls.Application.Ideas.Mappings;

public sealed class IdeaMappingProfile : Profile
{
    public IdeaMappingProfile()
    {
        CreateMap<Idea, IdeaDto>();

        CreateMap<Idea, IdeaWithPollDto>()
            .ForMember(dest => dest.Poll, opt => opt.MapFrom(src => src.Poll));
    }
}
