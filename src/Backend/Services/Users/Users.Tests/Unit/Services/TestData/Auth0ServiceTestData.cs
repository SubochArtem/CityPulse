using System.Net;
using Users.Business.DTOs;
using Users.Business.Services;
using Xunit;

namespace Users.Tests.Unit.Services.TestData;

public static class Auth0ServiceTestData
{
    private const string Nickname = "new_nickname";

    public static TheoryData<Func<Auth0Service, string, Task>> Operations => new()
    {
        (service, identityId) => service.BlockUserAsync(identityId),
        (service, identityId) => service.UnblockUserAsync(identityId),
        (service, identityId) => service.DeleteUserAsync(identityId),
        (service, identityId) => service.UpdateUserProfileAsync(
            identityId,
            new UpdateUserProfileDto { Nickname = Nickname })
    };

    public static TheoryData<HttpStatusCode> ManagementErrorStatusCodes => new()
    {
        HttpStatusCode.BadRequest,
        HttpStatusCode.Unauthorized,
        HttpStatusCode.Forbidden,
        HttpStatusCode.InternalServerError,
        HttpStatusCode.BadGateway,
        HttpStatusCode.ServiceUnavailable
    };

    public static TheoryData<HttpStatusCode> TokenErrorStatusCodes => new()
    {
        HttpStatusCode.Unauthorized,
        HttpStatusCode.Forbidden,
        HttpStatusCode.InternalServerError
    };
}
