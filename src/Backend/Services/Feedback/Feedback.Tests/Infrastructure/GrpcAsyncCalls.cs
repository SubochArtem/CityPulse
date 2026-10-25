using Grpc.Core;

namespace Feedback.Tests.Infrastructure;

internal static class GrpcAsyncCalls
{
    private const string FailureDetail = "grpc failure";

    public static AsyncUnaryCall<TResponse> Successful<TResponse>(TResponse response) =>
        new(
            Task.FromResult(response),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

    public static AsyncUnaryCall<TResponse> Failed<TResponse>(StatusCode statusCode) =>
        new(
            Task.FromException<TResponse>(new RpcException(new Status(statusCode, FailureDetail))),
            Task.FromResult(new Metadata()),
            () => new Status(statusCode, FailureDetail),
            () => new Metadata(),
            () => { });
}
