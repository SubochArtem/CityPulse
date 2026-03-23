using AutoMapper;
using Polls.Application.Polls.DTOs;
using Polls.Domain.Polls;

namespace Polls.Application.Polls.Mappings;

public sealed class PollMappingProfile : Profile
{
    public PollMappingProfile()
    {
        CreateMap<Poll, PollDto>();

        CreateMap<Poll, PollWithIdeasDto>()
            .ForMember(dest => dest.Ideas, opt => opt.MapFrom(src => src.Ideas));
    }
}
