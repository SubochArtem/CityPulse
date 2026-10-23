using Grpc.Core;
using Xunit;

namespace Users.Tests.Unit.Services.TestData;

public static class CityGrpcServiceTestData
{
    public static TheoryData<StatusCode> FailureStatusCodes => new()
    {
        StatusCode.NotFound,
        StatusCode.InvalidArgument,
        StatusCode.Unavailable,
        StatusCode.DeadlineExceeded
    };
}
