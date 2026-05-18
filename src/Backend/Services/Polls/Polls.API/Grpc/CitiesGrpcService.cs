using Grpc.Core;
using MediatR;
using Polls.Application.Cities.Queries.GetCityById;

namespace Polls.API.Grpc;

public class CitiesGrpcService(ISender sender) : CitiesService.CitiesServiceBase
{
    private readonly ISender _sender = sender;

    public override async Task<GetCityResponse> GetCity(
        GetCityRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.CityId, out var cityId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid city id format."));


        var query = new GetCityByIdQuery(cityId, IncludeOnlyActive: false);
        var result = await _sender.Send(query, context.CancellationToken);

        if (!result.IsSuccess)
            throw new RpcException(new Status(StatusCode.NotFound, $"City with id '{cityId}' was not found."));
        
        var city = result.Value!;

        return new GetCityResponse
        {
            Id = city.Id.ToString(),
            Name = city.Title, 
            Status = city.Status
        };
    }
}
