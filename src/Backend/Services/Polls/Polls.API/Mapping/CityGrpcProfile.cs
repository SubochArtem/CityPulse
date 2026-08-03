using AutoMapper;
using CityPulse.Contracts.Grpc.Protos;
using Polls.Application.Cities.DTOs;
using DomainCityStatus = Polls.Domain.Cities.Enums.CityStatus;
using GrpcCityStatus = CityPulse.Contracts.Grpc.Protos.CityStatus;

namespace Polls.API.Mapping;

public class CityGrpcProfile : Profile
{
    public CityGrpcProfile()
    {
        CreateMap<DomainCityStatus, GrpcCityStatus>()
            .ConvertUsing((status, _, _) => MapStatus(status));

        CreateMap<CityDto, GetCityResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));
    }

    private static GrpcCityStatus MapStatus(DomainCityStatus status)
    {
        return status switch
        {
            DomainCityStatus.Undefined => GrpcCityStatus.Undefined,
            DomainCityStatus.Active => GrpcCityStatus.Active,
            DomainCityStatus.Inactive => GrpcCityStatus.Inactive,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}
