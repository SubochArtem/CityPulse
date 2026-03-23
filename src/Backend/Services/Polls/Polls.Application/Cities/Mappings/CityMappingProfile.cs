using AutoMapper;
using Polls.Application.Cities.DTOs;
using Polls.Domain.Cities;

namespace Polls.Application.Cities.Mappings;

public class CityMappingProfile : Profile
{
    public CityMappingProfile()
    {
        CreateMap<Coordinates, CoordinatesDto>();

        CreateMap<City, CityDto>();

        CreateMap<City, CityWithPollsDto>()
            .ForMember(dest => dest.Polls, opt => opt.MapFrom(src => src.Polls));
    }
}
