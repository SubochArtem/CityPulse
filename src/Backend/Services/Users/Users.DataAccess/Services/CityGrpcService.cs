using CityPulse.Contracts.Grpc.Protos;
using Users.DataAccess.DTOs;
using Users.DataAccess.Interfaces;
using CityStatus = Users.DataAccess.DTOs.CityStatus;

namespace Users.DataAccess.Services;

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
