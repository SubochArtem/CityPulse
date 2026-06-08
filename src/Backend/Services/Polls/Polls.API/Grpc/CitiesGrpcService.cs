using CityPulse.Contracts.Grpc.Protos;
using Grpc.Core;
using MediatR;
using Polls.Application.Cities.Queries.GetCityById;

namespace Polls.API.Grpc;

public class CitiesGrpcService(ISender sender) : CitiesService.CitiesServiceBase
{
    private const string InvalidCityIdMessage = "Invalid city id format";
    private static string GetCityNotFoundMessage(Guid id) => $"City with id '{id}' was not found";
    
    public override async Task<GetCityResponse> GetCity(
        GetCityRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.CityId, out var cityId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, InvalidCityIdMessage));


        var query = new GetCityByIdQuery(cityId, IncludeOnlyActive: false);
        var result = await sender.Send(query, context.CancellationToken);

        if (!result.IsSuccess)
            throw new RpcException(new Status(StatusCode.NotFound, GetCityNotFoundMessage(cityId)));
        
        var city = result.Value!;

        return new GetCityResponse
        {
            Id = city.Id.ToString(),
            Name = city.Title, 
            Status = city.Status
        };
    }
}
