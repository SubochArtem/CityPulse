using Grpc.Core;
using Xunit;

namespace Feedback.Tests.Unit.Grpc.TestData;

public static class IdeaGrpcClientTestData
{
    public static TheoryData<StatusCode> IdeaNotFoundStatuses => new()
    {
        StatusCode.NotFound,
        StatusCode.InvalidArgument
    };

    public static TheoryData<StatusCode> ServiceUnavailableStatuses => new()
    {
        StatusCode.Unavailable,
        StatusCode.DeadlineExceeded
    };

    public static TheoryData<StatusCode> PropagatedStatuses => new()
    {
        StatusCode.Internal,
        StatusCode.Unauthenticated,
        StatusCode.PermissionDenied,
        StatusCode.Unknown,
        StatusCode.ResourceExhausted
    };

    public static TheoryData<bool> IncludeOnlyActiveValues => new()
    {
        true,
        false
    };
}
