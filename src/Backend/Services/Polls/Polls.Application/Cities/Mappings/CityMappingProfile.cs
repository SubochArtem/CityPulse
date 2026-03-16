using AutoMapper;
using Polls.Application.Cities.DTOs;
using Polls.Domain.Cities;

namespace Polls.Application.Cities.Mappings;

public class CityMappingProfile : Profile
{
    public CityMappingProfile()
    {
        CreateMap<Coordinates, CoordinatesDto>();

        CreateMap<City, CityDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<City, CityWithPollsDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Polls, opt => opt.MapFrom(src => src.Polls));
    }
}
