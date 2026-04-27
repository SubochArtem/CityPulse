using AutoMapper;
using Polls.Application.Cities.DTOs;
using Polls.Application.Images.DTOs;
using Polls.Application.Images.Resolvers;
using Polls.Domain.Cities;
using Polls.Domain.Images;

namespace Polls.Application.Cities.Mappings;

public class CityMappingProfile : Profile
{
    public CityMappingProfile()
    {
        CreateMap<Coordinates, CoordinatesDto>();
        CreateMap<CityImage, ImageDto>()
            .ForMember(d => d.Url, opt => opt.MapFrom<ImageUrlResolver>());
        CreateMap<City, CityDto>()
            .ForMember(d => d.Images, opt => opt.MapFrom(src => src.Images));
        CreateMap<City, CityWithPollsDto>()
            .ForMember(dest => dest.Coordinates, opt => opt.MapFrom(src => src.Coordinates))
            .ForMember(dest => dest.Polls, opt => opt.MapFrom(src => src.Polls))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images));
    }
}
