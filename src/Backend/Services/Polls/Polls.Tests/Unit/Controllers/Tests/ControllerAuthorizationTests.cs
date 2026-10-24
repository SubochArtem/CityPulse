using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Polls.API.Controllers.Admin;
using Polls.Tests.Unit.Controllers.TestData;
using Xunit;

namespace Polls.Tests.Unit.Controllers.Tests;

public sealed class ControllerAuthorizationTests
{
    private static MethodInfo FindEndpoint(Type controller, string methodName) =>
        Assert.Single(
            controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            method => method.Name == methodName);

    private static IEnumerable<AuthorizeAttribute> AuthorizeOn(MethodInfo method) =>
        method.GetCustomAttributes<AuthorizeAttribute>(inherit: true);

    [Theory]
    [MemberData(nameof(ControllerAuthorizationTestData.ProtectedEndpoints), MemberType = typeof(ControllerAuthorizationTestData))]
    public void Endpoint_IsProtected_RequiresExpectedPolicy(Type controller, string methodName, string expectedPolicy)
    {
        var endpoint = FindEndpoint(controller, methodName);

        var policies = AuthorizeOn(endpoint).Select(attribute => attribute.Policy);

        Assert.Contains(expectedPolicy, policies);
    }

    [Theory]
    [MemberData(nameof(ControllerAuthorizationTestData.AdminControllerTypes), MemberType = typeof(ControllerAuthorizationTestData))]
    public void AdminController_Always_RequiresAuthenticationOnClass(Type controller)
    {
        var authorize = controller.GetCustomAttribute<AuthorizeAttribute>(inherit: true);
        var allowAnonymous = controller.GetCustomAttribute<AllowAnonymousAttribute>(inherit: true);

        Assert.NotNull(authorize);
        Assert.Null(allowAnonymous);
    }

    [Theory]
    [MemberData(nameof(ControllerAuthorizationTestData.PublicReads), MemberType = typeof(ControllerAuthorizationTestData))]
    public void PublicRead_Endpoint_DoesNotRequireAuthorization(Type controller, string methodName)
    {
        var endpoint = FindEndpoint(controller, methodName);

        Assert.Empty(AuthorizeOn(endpoint));
        Assert.Null(controller.GetCustomAttribute<AuthorizeAttribute>(inherit: true));
    }

    [Fact]
    public void Controllers_Always_AreAllKnownToAuthorizationTable()
    {
        var known = ControllerAuthorizationTestData.AdminControllers
            .Concat(ControllerAuthorizationTestData.UserControllers)
            .ToHashSet();

        var actual = typeof(AdminCitiesController).Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && typeof(ControllerBase).IsAssignableFrom(type))
            .ToHashSet();

        Assert.Empty(actual.Except(known).Select(type => type.Name));
        Assert.Empty(known.Except(actual).Select(type => type.Name));
    }

    [Fact]
    public void Endpoints_Always_AreAllCoveredByAuthorizationTable()
    {
        var covered = ControllerAuthorizationTestData.ProtectedEndpointDefinitions
            .Select(endpoint => (endpoint.Controller, endpoint.Method))
            .Concat(ControllerAuthorizationTestData.PublicReadDefinitions)
            .ToHashSet();

        var actual = ControllerAuthorizationTestData.AdminControllers
            .Concat(ControllerAuthorizationTestData.UserControllers)
            .SelectMany(controller => controller
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(method => method.GetCustomAttributes<HttpMethodAttribute>(inherit: true).Any())
                .Select(method => (Controller: controller, Method: method.Name)))
            .ToHashSet();

        Assert.Empty(actual.Except(covered).Select(endpoint => $"{endpoint.Controller.Name}.{endpoint.Method}"));
        Assert.Empty(covered.Except(actual).Select(endpoint => $"{endpoint.Controller.Name}.{endpoint.Method}"));
    }
}
