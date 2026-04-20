using AutoMapper;
using Polls.Application.Polls.Commands.CreatePoll;
using Polls.Application.Polls.DTOs;
using Polls.Domain.Polls;
using Polls.Domain.Polls.Enums;

namespace Polls.Application.Polls.Mappings;

public sealed class PollMappingProfile : Profile
{
    public PollMappingProfile()
    {
        CreateMap<Poll, PollDto>();
        
        CreateMap<CreatePollCommand, Poll>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => PollStatus.Active))
            .ForMember(dest => dest.Ideas, opt => opt.Ignore());

        CreateMap<Poll, PollWithIdeasDto>()
            .ForMember(dest => dest.Ideas, opt => opt.MapFrom(src => src.Ideas));
    }
}
