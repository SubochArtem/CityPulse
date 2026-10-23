using AutoMapper;
using CityPulse.Contracts.Grpc.Protos;
using Grpc.Core;
using MediatR;
using Polls.Application.Ideas.Queries.GetIdeaCity;

namespace Polls.API.Grpc;

public class IdeasGrpcService(
    ISender sender,
    IMapper mapper) : IdeasService.IdeasServiceBase
{
    private const string InvalidIdeaIdMessage = "Invalid idea id format";

    private static string GetIdeaNotFoundMessage(Guid id)
    {
        return $"Idea with id '{id}' was not found";
    }

    public override async Task<GetIdeaCityResponse> GetIdeaCity(
        GetIdeaCityRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.IdeaId, out var ideaId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, InvalidIdeaIdMessage));

        var query = new GetIdeaCityQuery(ideaId, request.IncludeOnlyActive);
        var result = await sender.Send(query, context.CancellationToken);

        if (!result.IsSuccess)
            throw new RpcException(new Status(StatusCode.NotFound, GetIdeaNotFoundMessage(ideaId)));

        return mapper.Map<GetIdeaCityResponse>(result.Value!);
    }
}
