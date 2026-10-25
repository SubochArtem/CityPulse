using System.Reflection;
using Feedback.API.Features.Feedback.AddFeedback;
using Feedback.Tests.Unit.Controllers.TestData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Xunit;

namespace Feedback.Tests.Unit.Controllers.Tests;

public sealed class ControllerAuthorizationTests
{
    private const string AdminRoutePrefix = "api/v1/admin/";
    private const string AdminSegment = "admin";

    private static IEnumerable<MethodInfo> ActionsOf(Type controller) =>
        controller
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(method => method.GetCustomAttributes<HttpMethodAttribute>(inherit: true).Any());

    private static MethodInfo FindEndpoint(Type controller, string methodName) =>
        Assert.Single(ActionsOf(controller), method => method.Name == methodName);

    [Theory]
    [MemberData(nameof(ControllerAuthorizationTestData.Endpoints), MemberType = typeof(ControllerAuthorizationTestData))]
    public void Endpoint_Always_RequiresExpectedPolicy(Type controller, string methodName, string expectedPolicy)
    {
        var endpoint = FindEndpoint(controller, methodName);

        var policies = endpoint
            .GetCustomAttributes<AuthorizeAttribute>(inherit: true)
            .Select(attribute => attribute.Policy);

        Assert.Contains(expectedPolicy, policies);
    }

    [Theory]
    [MemberData(nameof(ControllerAuthorizationTestData.AllControllerTypes), MemberType = typeof(ControllerAuthorizationTestData))]
    public void Controller_Always_DoesNotAllowAnonymousAccess(Type controller)
    {
        Assert.Null(controller.GetCustomAttribute<AllowAnonymousAttribute>(inherit: true));

        foreach (var action in ActionsOf(controller))
            Assert.Null(action.GetCustomAttribute<AllowAnonymousAttribute>(inherit: true));
    }

    [Theory]
    [MemberData(nameof(ControllerAuthorizationTestData.AdminControllerTypes), MemberType = typeof(ControllerAuthorizationTestData))]
    public void AdminController_Always_IsRoutedUnderAdminPrefix(Type controller)
    {
        var route = controller.GetCustomAttribute<RouteAttribute>(inherit: true);

        Assert.NotNull(route);
        Assert.StartsWith(AdminRoutePrefix, route.Template);
    }

    [Theory]
    [MemberData(nameof(ControllerAuthorizationTestData.UserControllerTypes), MemberType = typeof(ControllerAuthorizationTestData))]
    public void UserController_Always_IsNotRoutedUnderAdminPrefix(Type controller)
    {
        var route = controller.GetCustomAttribute<RouteAttribute>(inherit: true);

        Assert.NotNull(route);
        Assert.DoesNotContain(AdminSegment, route.Template);
    }

    [Fact]
    public void Controllers_Always_AreAllKnownToAuthorizationTable()
    {
        var known = ControllerAuthorizationTestData.AdminControllers
            .Concat(ControllerAuthorizationTestData.UserControllers)
            .ToHashSet();

        var actual = typeof(UserAddFeedbackController).Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && typeof(ControllerBase).IsAssignableFrom(type))
            .ToHashSet();

        Assert.Empty(actual.Except(known).Select(type => type.Name));
        Assert.Empty(known.Except(actual).Select(type => type.Name));
    }

    [Fact]
    public void Endpoints_Always_AreAllCoveredByAuthorizationTable()
    {
        var covered = ControllerAuthorizationTestData.EndpointDefinitions
            .Select(endpoint => (endpoint.Controller, endpoint.Method))
            .ToHashSet();

        var actual = ControllerAuthorizationTestData.AdminControllers
            .Concat(ControllerAuthorizationTestData.UserControllers)
            .SelectMany(controller => ActionsOf(controller).Select(method => (Controller: controller, Method: method.Name)))
            .ToHashSet();

        Assert.Empty(actual.Except(covered).Select(endpoint => $"{endpoint.Controller.Name}.{endpoint.Method}"));
        Assert.Empty(covered.Except(actual).Select(endpoint => $"{endpoint.Controller.Name}.{endpoint.Method}"));
    }
}
