using CityPulse.Contracts.Grpc.Protos;
using Users.Business.DTOs;
using Users.Business.Interfaces;

namespace Users.Business.Services;

public class CityGrpcService(CitiesService.CitiesServiceClient client) : ICityService
{
    public async Task<CityDto> GetCityAsync(
        Guid cityId,
        CancellationToken cancellationToken = default)
    {
        var response = await client.GetCityAsync(
            new GetCityRequest { CityId = cityId.ToString() },
            cancellationToken: cancellationToken);

        return new CityDto
        {
            Id = Guid.Parse(response.Id),
            Name = response.Name,
            Status = (CityStatus)response.Status
        };
    }
}
