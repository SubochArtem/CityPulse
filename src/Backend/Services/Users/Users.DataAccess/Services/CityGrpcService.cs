using CityPulse.Contracts.Grpc.Protos;
using MapsterMapper;
using Users.DataAccess.DTOs;
using Users.DataAccess.Interfaces;

namespace Users.DataAccess.Services;

public class CityGrpcService(
    CitiesService.CitiesServiceClient client,
    IMapper mapper) : ICityService
{
    public async Task<CityDto> GetCityAsync(
        Guid cityId,
        CancellationToken cancellationToken = default)
    {
        var response = await client.GetCityAsync(
            new GetCityRequest { CityId = cityId.ToString() },
            cancellationToken: cancellationToken);

        return mapper.Map<CityDto>(response);
    }
}
