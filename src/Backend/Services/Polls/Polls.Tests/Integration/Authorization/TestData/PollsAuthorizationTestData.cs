using Polls.Domain.Authorization;
using Xunit;

namespace Polls.Tests.Integration.Authorization.TestData;

public sealed record EndpointCase(HttpMethod Method, string Route, string Policy)
{
    public override string ToString() => $"{Method} {Route}";
}

public static class PollsAuthorizationTestData
{
    private const string UserCities = "api/v1/cities";
    private const string UserPolls = "api/v1/polls";
    private const string UserIdeas = "api/v1/ideas";
    private const string AdminCities = "api/v1/admin/cities";
    private const string AdminPolls = "api/v1/admin/polls";
    private const string AdminIdeas = "api/v1/admin/ideas";
    private const string UnrelatedPermission = "unrelated.permission";

    private static readonly string Id = Guid.Parse("33333333-3333-3333-3333-333333333333").ToString();

    private static readonly EndpointCase[] ProtectedDefinitions =
    [
        new(HttpMethod.Post, $"{UserPolls}/cities/{Id}", Permissions.Polls.CreateCity),
        new(HttpMethod.Put, $"{UserPolls}/{Id}", Permissions.Polls.UpdateCity),
        new(HttpMethod.Delete, $"{UserPolls}/{Id}", Permissions.Polls.DeleteCity),

        new(HttpMethod.Post, $"{UserIdeas}/polls/{Id}", Permissions.Ideas.CreateCity),
        new(HttpMethod.Put, $"{UserIdeas}/{Id}", Permissions.Ideas.UpdateOwn),
        new(HttpMethod.Delete, $"{UserIdeas}/{Id}", Permissions.Ideas.DeleteOwn),

        new(HttpMethod.Get, AdminCities, Permissions.Cities.ReadAny),
        new(HttpMethod.Get, $"{AdminCities}/{Id}", Permissions.Cities.ReadAny),
        new(HttpMethod.Get, $"{AdminCities}/{Id}/polls", Permissions.Cities.ReadAny),
        new(HttpMethod.Post, AdminCities, Permissions.Cities.CreateAny),
        new(HttpMethod.Put, $"{AdminCities}/{Id}", Permissions.Cities.UpdateAny),
        new(HttpMethod.Delete, $"{AdminCities}/{Id}", Permissions.Cities.DeleteAny),
        new(HttpMethod.Patch, $"{AdminCities}/{Id}/status", Permissions.Cities.ChangeStatusAny),

        new(HttpMethod.Get, AdminPolls, Permissions.Polls.ReadAny),
        new(HttpMethod.Get, $"{AdminPolls}/{Id}", Permissions.Polls.ReadAny),
        new(HttpMethod.Get, $"{AdminPolls}/{Id}/ideas", Permissions.Polls.ReadAny),
        new(HttpMethod.Post, $"{AdminPolls}/cities/{Id}", Permissions.Polls.CreateAny),
        new(HttpMethod.Put, $"{AdminPolls}/{Id}", Permissions.Polls.UpdateAny),
        new(HttpMethod.Delete, $"{AdminPolls}/{Id}", Permissions.Polls.DeleteAny),
        new(HttpMethod.Patch, $"{AdminPolls}/{Id}/status", Permissions.Polls.ChangeStatusAny),

        new(HttpMethod.Get, AdminIdeas, Permissions.Ideas.ReadAny),
        new(HttpMethod.Get, $"{AdminIdeas}/{Id}", Permissions.Ideas.ReadAny),
        new(HttpMethod.Get, $"{AdminIdeas}/{Id}/poll", Permissions.Ideas.ReadAny),
        new(HttpMethod.Post, $"{AdminIdeas}/{Id}", Permissions.Ideas.CreateAny),
        new(HttpMethod.Put, $"{AdminIdeas}/{Id}", Permissions.Ideas.UpdateAny),
        new(HttpMethod.Delete, $"{AdminIdeas}/{Id}", Permissions.Ideas.DeleteAny),
        new(HttpMethod.Patch, $"{AdminIdeas}/{Id}/access-status", Permissions.Ideas.ChangeAccessStatusAny),
        new(HttpMethod.Patch, $"{AdminIdeas}/{Id}/approval-status", Permissions.Ideas.ChangeApprovalStatusAny)
    ];

    private static readonly EndpointCase[] PublicDefinitions =
    [
        new(HttpMethod.Get, UserCities, string.Empty),
        new(HttpMethod.Get, $"{UserCities}/{Id}", string.Empty),
        new(HttpMethod.Get, $"{UserCities}/{Id}/polls", string.Empty),
        new(HttpMethod.Get, UserPolls, string.Empty),
        new(HttpMethod.Get, $"{UserPolls}/{Id}", string.Empty),
        new(HttpMethod.Get, $"{UserPolls}/{Id}/ideas", string.Empty),
        new(HttpMethod.Get, UserIdeas, string.Empty),
        new(HttpMethod.Get, $"{UserIdeas}/{Id}", string.Empty),
        new(HttpMethod.Get, $"{UserIdeas}/{Id}/poll", string.Empty)
    ];

    private static readonly EndpointCase[] CrossOverDefinitions =
    [
        new(HttpMethod.Post, $"{AdminPolls}/cities/{Id}", Permissions.Polls.CreateCity),
        new(HttpMethod.Put, $"{AdminPolls}/{Id}", Permissions.Polls.UpdateCity),
        new(HttpMethod.Delete, $"{AdminPolls}/{Id}", Permissions.Polls.DeleteCity),
        new(HttpMethod.Post, $"{AdminIdeas}/{Id}", Permissions.Ideas.CreateCity),
        new(HttpMethod.Put, $"{AdminIdeas}/{Id}", Permissions.Ideas.UpdateOwn),
        new(HttpMethod.Delete, $"{AdminIdeas}/{Id}", Permissions.Ideas.DeleteOwn),
        new(HttpMethod.Post, $"{UserPolls}/cities/{Id}", Permissions.Polls.CreateAny),
        new(HttpMethod.Put, $"{UserPolls}/{Id}", Permissions.Polls.UpdateAny),
        new(HttpMethod.Delete, $"{UserPolls}/{Id}", Permissions.Polls.DeleteAny),
        new(HttpMethod.Post, $"{UserIdeas}/polls/{Id}", Permissions.Ideas.CreateAny),
        new(HttpMethod.Put, $"{UserIdeas}/{Id}", Permissions.Ideas.UpdateAny),
        new(HttpMethod.Delete, $"{UserIdeas}/{Id}", Permissions.Ideas.DeleteAny)
    ];

    public static string Unrelated => UnrelatedPermission;

    public static TheoryData<EndpointCase> ProtectedEndpoints => ToTheoryData(ProtectedDefinitions);

    public static TheoryData<EndpointCase> PublicEndpoints => ToTheoryData(PublicDefinitions);

    public static TheoryData<EndpointCase> CrossOverEndpoints => ToTheoryData(CrossOverDefinitions);

    private static TheoryData<EndpointCase> ToTheoryData(IEnumerable<EndpointCase> cases)
    {
        var data = new TheoryData<EndpointCase>();

        foreach (var endpointCase in cases)
            data.Add(endpointCase);

        return data;
    }
}
