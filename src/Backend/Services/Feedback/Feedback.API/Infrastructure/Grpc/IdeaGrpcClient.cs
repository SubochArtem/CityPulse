using CityPulse.Contracts.Grpc.Protos;
using ErrorOr;
using Feedback.API.Common.Errors;
using Feedback.API.Common.Interfaces;
using Grpc.Core;

namespace Feedback.API.Infrastructure.Grpc;

public class IdeaGrpcClient(IdeasService.IdeasServiceClient client) : IIdeaService
{
    public async Task<ErrorOr<Guid>> GetIdeaCityAsync(
        Guid ideaId,
        bool includeOnlyActive,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await client.GetIdeaCityAsync(
                new GetIdeaCityRequest
                {
                    IdeaId = ideaId.ToString(),
                    IncludeOnlyActive = includeOnlyActive
                },
                cancellationToken: cancellationToken);

            return Guid.Parse(response.CityId);
        }
        catch (RpcException ex) when (ex.StatusCode is StatusCode.NotFound or StatusCode.InvalidArgument)
        {
            return FeedbackErrors.IdeaNotFound;
        }
        catch (RpcException ex) when (ex.StatusCode is StatusCode.Unavailable or StatusCode.DeadlineExceeded)
        {
            return FeedbackErrors.IdeasServiceUnavailable;
        }
    }
}
