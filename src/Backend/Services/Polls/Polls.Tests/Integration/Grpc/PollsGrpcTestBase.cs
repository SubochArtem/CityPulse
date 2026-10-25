using CityPulse.Contracts.Grpc.Protos;
using Grpc.Net.Client;
using Polls.Tests.Infrastructure;

namespace Polls.Tests.Integration.Grpc;

public abstract class PollsGrpcTestBase(PollsWebAppFactory factory) : PollsApiTestBase(factory)
{
    private static readonly Version Http2 = new(2, 0);

    protected CitiesService.CitiesServiceClient CreateCitiesClient() => new(CreateChannel());

    protected IdeasService.IdeasServiceClient CreateIdeasClient() => new(CreateChannel());

    private GrpcChannel CreateChannel()
    {
        var httpClient = Factory.CreateDefaultClient(new ResponseVersionHandler());

        return GrpcChannel.ForAddress(
            httpClient.BaseAddress!,
            new GrpcChannelOptions { HttpClient = httpClient });
    }

    private sealed class ResponseVersionHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            response.Version = Http2;

            return response;
        }
    }
}
