using System.Net;
using Xunit;

namespace Users.Tests.Unit.Policies.TestData;

public static class HttpPoliciesTestData
{
    public static TheoryData<HttpStatusCode> RetryableStatusCodes => new()
    {
        HttpStatusCode.InternalServerError,
        HttpStatusCode.BadGateway,
        HttpStatusCode.ServiceUnavailable,
        HttpStatusCode.TooManyRequests,
        HttpStatusCode.RequestTimeout,
        HttpStatusCode.NotFound
    };
}
