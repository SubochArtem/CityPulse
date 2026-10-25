using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Polls.API.Common.Filters;
using Polls.Domain.Common;
using Polls.Domain.Common.Enums;
using Polls.Tests.Unit.Filters.TestData;
using Xunit;

namespace Polls.Tests.Unit.Filters.Tests;

public sealed class ResultFilterTests
{
    private const string RequestPath = "/api/v1/polls/42";
    private const string ErrorsKey = "errors";
    private const string FirstDescription = "first problem";
    private const string SecondDescription = "second problem";
    private const string ThirdDescription = "third problem";

    private readonly ResultFilter _sut = new();

    private static ActionContext BuildActionContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = RequestPath;

        return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
    }

    private static ActionExecutedContext BuildExecutedContext(IActionResult? result) =>
        new(BuildActionContext(), [], new object()) { Result = result };

    private static ObjectResult ResultOf(ActionExecutedContext context) =>
        Assert.IsType<ObjectResult>(context.Result);

    private static ProblemDetails ProblemOf(ActionExecutedContext context) =>
        Assert.IsType<ProblemDetails>(ResultOf(context).Value);

    private static ObjectResult FailureResult(params Error[] errors) =>
        new(Result<string>.Failure(errors));

    [Theory]
    [MemberData(nameof(ResultFilterTestData.ErrorStatusMappings), MemberType = typeof(ResultFilterTestData))]
    public void OnActionExecuted_ResultFailed_ReplacesResultWithMatchingStatusCode(ErrorType type, int expectedStatus)
    {
        var context = BuildExecutedContext(FailureResult(new Error(FirstDescription, type)));

        _sut.OnActionExecuted(context);

        Assert.Equal(expectedStatus, ResultOf(context).StatusCode);
        Assert.Equal(expectedStatus, ProblemOf(context).Status);
    }

    [Fact]
    public void OnActionExecuted_ResultFailed_FillsProblemDetailsFromFirstError()
    {
        var context = BuildExecutedContext(FailureResult(Error.NotFound(FirstDescription)));

        _sut.OnActionExecuted(context);

        var problem = ProblemOf(context);
        Assert.Equal(nameof(ErrorType.NotFound), problem.Title);
        Assert.Equal(FirstDescription, problem.Detail);
        Assert.Equal(RequestPath, problem.Instance);
    }

    [Fact]
    public void OnActionExecuted_SingleError_DoesNotAddErrorsExtension()
    {
        var context = BuildExecutedContext(FailureResult(Error.Conflict(FirstDescription)));

        _sut.OnActionExecuted(context);

        Assert.DoesNotContain(ErrorsKey, ProblemOf(context).Extensions.Keys);
    }

    [Fact]
    public void OnActionExecuted_SeveralErrors_AddsAllDescriptionsInOrder()
    {
        var context = BuildExecutedContext(FailureResult(
            new Error(FirstDescription, ErrorType.Validation),
            new Error(SecondDescription, ErrorType.Validation),
            new Error(ThirdDescription, ErrorType.Validation)));

        _sut.OnActionExecuted(context);

        var problem = ProblemOf(context);
        Assert.Equal(FirstDescription, problem.Detail);
        var errors = Assert.IsType<string[]>(problem.Extensions[ErrorsKey]);
        Assert.Equal([FirstDescription, SecondDescription, ThirdDescription], errors);
    }

    [Fact]
    public void OnActionExecuted_ResultSucceededWithValue_ReplacesResultWithOkContainingValue()
    {
        const string value = "value";
        var context = BuildExecutedContext(new ObjectResult(Result<string>.Success(value)));

        _sut.OnActionExecuted(context);

        var ok = Assert.IsType<OkObjectResult>(context.Result);
        Assert.Same(value, ok.Value);
    }

    [Fact]
    public void OnActionExecuted_ResultSucceededWithUnit_ReplacesResultWithNoContent()
    {
        var context = BuildExecutedContext(new ObjectResult(Result<MediatR.Unit>.Success(MediatR.Unit.Value)));

        _sut.OnActionExecuted(context);

        Assert.IsType<NoContentResult>(context.Result);
    }

    [Fact]
    public void OnActionExecuted_ResultSucceededWithNullValue_ReplacesResultWithNoContent()
    {
        var context = BuildExecutedContext(new ObjectResult(Result<string?>.Success(null)));

        _sut.OnActionExecuted(context);

        Assert.IsType<NoContentResult>(context.Result);
    }

    [Fact]
    public void OnActionExecuted_ValueIsNotResult_LeavesResultUnchanged()
    {
        var original = new ObjectResult("plain value");
        var context = BuildExecutedContext(original);

        _sut.OnActionExecuted(context);

        Assert.Same(original, context.Result);
    }

    [Fact]
    public void OnActionExecuted_ResultIsNotObjectResult_LeavesResultUnchanged()
    {
        var original = new OkResult();
        var context = BuildExecutedContext(original);

        _sut.OnActionExecuted(context);

        Assert.Same(original, context.Result);
    }

    [Fact]
    public void OnActionExecuted_ResultIsNull_DoesNotThrow()
    {
        var context = BuildExecutedContext(null);

        var exception = Record.Exception(() => _sut.OnActionExecuted(context));

        Assert.Null(exception);
        Assert.Null(context.Result);
    }

    [Fact]
    public void OnActionExecuting_Always_DoesNothing()
    {
        var context = new ActionExecutingContext(
            BuildActionContext(),
            [],
            new Dictionary<string, object?>(),
            new object());

        var exception = Record.Exception(() => _sut.OnActionExecuting(context));

        Assert.Null(exception);
        Assert.Null(context.Result);
    }
}
