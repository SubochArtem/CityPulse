using CityPulse.Contracts.Grpc.Protos;
using Mapster;
using CityDto = Users.DataAccess.DTOs.CityDto;
using GrpcCityStatus = CityPulse.Contracts.Grpc.Protos.CityStatus;
using DtoCityStatus = Users.DataAccess.DTOs.CityStatus;

namespace Users.DataAccess.Mapping;

public class CityMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<GrpcCityStatus, DtoCityStatus>()
            .MapWith(status => MapStatus(status));

        config.NewConfig<GetCityResponse, CityDto>()
            .Map(dest => dest.Id, src => Guid.Parse(src.Id))
            .Map(dest => dest.Name, src => src.Name);
    }

    private static DtoCityStatus MapStatus(GrpcCityStatus status)
    {
        return status switch
        {
            GrpcCityStatus.Undefined => DtoCityStatus.Undefined,
            GrpcCityStatus.Active => DtoCityStatus.Active,
            GrpcCityStatus.Inactive => DtoCityStatus.Inactive,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}
