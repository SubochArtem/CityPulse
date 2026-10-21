using System.Net;
using Grpc.Core;
using Users.DataAccess.DTOs;
using Users.Presentation.Middleware;
using Xunit;

namespace Users.Tests.Integration.Users.TestData;

public static class UpdateUserErrorHandlingTestData
{
    public const string NewNickname = "new_nickname";
    public const string Auth0FailureMessage = "Auth0 is down";

    public static TheoryData<CityStatus> NotActiveCityStatuses => new()
    {
        CityStatus.Inactive,
        CityStatus.Undefined
    };

    public static TheoryData<StatusCode, HttpStatusCode, string> CitiesServiceFailures => new()
    {
        { StatusCode.NotFound, HttpStatusCode.NotFound, MiddlewareExceptionMessages.Titles.CityNotFound },
        {
            StatusCode.Unavailable,
            HttpStatusCode.ServiceUnavailable,
            MiddlewareExceptionMessages.Titles.CitiesServiceUnavailable
        },
        {
            StatusCode.DeadlineExceeded,
            HttpStatusCode.GatewayTimeout,
            MiddlewareExceptionMessages.Titles.CitiesServiceTimeout
        }
    };
}
