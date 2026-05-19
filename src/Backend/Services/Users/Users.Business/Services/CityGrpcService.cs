using CityPulse.Contracts.Cities;
using Grpc.Core;
using Users.Business.DTOs;
using Users.Business.Interfaces;

namespace Users.Business.Services;

public class CityGrpcService(CitiesService.CitiesServiceClient client) : ICityService
{
    private readonly CitiesService.CitiesServiceClient _client = client;

    public async Task<CityDto?> GetCityAsync(
        Guid cityId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.GetCityAsync(
                new GetCityRequest { CityId = cityId.ToString() },
                cancellationToken: cancellationToken);

            return new CityDto
            {
                Id = Guid.Parse(response.Id),
                Name = response.Name,
                Status = (CityStatus)response.Status
            };
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }
}
