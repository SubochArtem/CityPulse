using Users.Tests.Infrastructure.Constants;
using Xunit;

namespace Users.Tests.Integration.Authorization.TestData;

public static class UsersAuthorizationTestData
{
    public const string UserIdPlaceholder = "{id}";
    public const string UsersRoute = "api/v1/Users";
    public const string UserRoute = $"{UsersRoute}/{UserIdPlaceholder}";
    public const string ActivateRoute = $"{UserRoute}/activate";
    public const string DeactivateRoute = $"{UserRoute}/deactivate";

    private const string Get = "GET";
    private const string Patch = "PATCH";
    private const string Post = "POST";
    private const string Delete = "DELETE";

    private static readonly string[] AllPermissions =
    [
        AuthConstants.Permissions.ReadMe,
        AuthConstants.Permissions.ReadAny,
        AuthConstants.Permissions.UpdateMe,
        AuthConstants.Permissions.UpdateAny,
        AuthConstants.Permissions.DeleteAny,
        AuthConstants.Permissions.ActivateAny,
        AuthConstants.Permissions.DeactivateAny
    ];

    private static readonly (string Method, string Route, string[] GrantingPermissions)[] Endpoints =
    [
        (Get, UserRoute, [AuthConstants.Permissions.ReadMe, AuthConstants.Permissions.ReadAny]),
        (Get, UsersRoute, [AuthConstants.Permissions.ReadAny]),
        (Patch, UserRoute, [AuthConstants.Permissions.UpdateMe, AuthConstants.Permissions.UpdateAny]),
        (Post, ActivateRoute, [AuthConstants.Permissions.ActivateAny]),
        (Post, DeactivateRoute, [AuthConstants.Permissions.DeactivateAny]),
        (Delete, UserRoute, [AuthConstants.Permissions.DeleteAny])
    ];

    public static TheoryData<string, string> AllEndpoints
    {
        get
        {
            var data = new TheoryData<string, string>();
            foreach (var (method, route, _) in Endpoints)
            {
                data.Add(method, route);
            }

            return data;
        }
    }

    public static TheoryData<string, string, string> EndpointsWithoutAccessPermissions
    {
        get
        {
            var data = new TheoryData<string, string, string>();
            foreach (var (method, route, granting) in Endpoints)
            {
                data.Add(method, route, string.Empty);
                data.Add(method, route, JoinPermissions(AllPermissions.Except(granting)));
            }

            return data;
        }
    }

    public static TheoryData<string, string, string> EndpointsWithAnyPermission
    {
        get
        {
            var data = new TheoryData<string, string, string>();
            foreach (var (method, route, granting) in Endpoints)
            {
                data.Add(method, route, granting[^1]);
            }

            return data;
        }
    }

    public static TheoryData<string, string, string> SelfEndpoints => new()
    {
        { Get, UserRoute, AuthConstants.Permissions.ReadMe },
        { Patch, UserRoute, AuthConstants.Permissions.UpdateMe }
    };

    public static TheoryData<string, string, string, string?> SelfEndpointsWithInvalidInternalUserId
    {
        get
        {
            var data = new TheoryData<string, string, string, string?>();
            foreach (var (method, route, permission) in SelfEndpointRows)
            {
                data.Add(method, route, permission, null);
                data.Add(method, route, permission, "not-a-guid");
            }

            return data;
        }
    }

    private static IEnumerable<(string Method, string Route, string Permission)> SelfEndpointRows =>
    [
        (Get, UserRoute, AuthConstants.Permissions.ReadMe),
        (Patch, UserRoute, AuthConstants.Permissions.UpdateMe)
    ];

    private static string JoinPermissions(IEnumerable<string> permissions) =>
        string.Join(AuthConstants.Headers.PermissionsSeparator, permissions);
}
