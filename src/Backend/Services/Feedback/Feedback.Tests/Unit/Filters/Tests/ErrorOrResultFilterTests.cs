using ErrorOr;
using Feedback.API.Common.Filters;
using Feedback.Tests.Unit.Filters.TestData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Feedback.Tests.Unit.Filters.Tests;

public sealed class ErrorOrResultFilterTests
{
    private const string RequestPath = "/api/v1/feedback/42";
    private const string ErrorsKey = "errors";
    private const string ErrorCode = "Feedback.Test";
    private const string FirstDescription = "first problem";
    private const string SecondDescription = "second problem";
    private const string ThirdDescription = "third problem";
    private const string SuccessValue = "value";
    private const int NumberValue = 42;

    private readonly ErrorOrResultFilter _sut = new();

    private static ActionContext BuildActionContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = RequestPath;

        return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
    }

    private static ActionExecutedContext BuildExecutedContext(IActionResult? result) =>
        new(BuildActionContext(), [], new object()) { Result = result };

    private static ObjectResult ObjectResultOf(ActionExecutedContext context) =>
        Assert.IsType<ObjectResult>(context.Result);

    private static ProblemDetails ProblemOf(ActionExecutedContext context) =>
        Assert.IsType<ProblemDetails>(ObjectResultOf(context).Value);

    private static ObjectResult FailureResult(params Error[] errors)
    {
        ErrorOr<string> result = errors.ToList();

        return new ObjectResult(result);
    }

    [Theory]
    [MemberData(nameof(ErrorOrResultFilterTestData.ErrorStatusMappings), MemberType = typeof(ErrorOrResultFilterTestData))]
    public void OnActionExecuted_ResultIsError_ReplacesResultWithMatchingStatusCode(ErrorType type, int expectedStatus)
    {
        var context = BuildExecutedContext(FailureResult(Error.Custom((int)type, ErrorCode, FirstDescription)));

        _sut.OnActionExecuted(context);

        Assert.Equal(expectedStatus, ObjectResultOf(context).StatusCode);
        Assert.Equal(expectedStatus, ProblemOf(context).Status);
    }

    [Fact]
    public void OnActionExecuted_ResultIsError_FillsProblemDetailsFromFirstError()
    {
        var context = BuildExecutedContext(FailureResult(Error.NotFound(ErrorCode, FirstDescription)));

        _sut.OnActionExecuted(context);

        var problem = ProblemOf(context);
        Assert.Equal(nameof(ErrorType.NotFound), problem.Title);
        Assert.Equal(FirstDescription, problem.Detail);
        Assert.Equal(RequestPath, problem.Instance);
    }

    [Fact]
    public void OnActionExecuted_SingleError_DoesNotAddErrorsExtension()
    {
        var context = BuildExecutedContext(FailureResult(Error.Conflict(ErrorCode, FirstDescription)));

        _sut.OnActionExecuted(context);

        Assert.DoesNotContain(ErrorsKey, ProblemOf(context).Extensions.Keys);
    }

    [Fact]
    public void OnActionExecuted_SeveralErrors_AddsAllDescriptionsInOrder()
    {
        var context = BuildExecutedContext(FailureResult(
            Error.Validation(ErrorCode, FirstDescription),
            Error.Validation(ErrorCode, SecondDescription),
            Error.Validation(ErrorCode, ThirdDescription)));

        _sut.OnActionExecuted(context);

        var problem = ProblemOf(context);
        Assert.Equal(FirstDescription, problem.Detail);
        var errors = Assert.IsType<string[]>(problem.Extensions[ErrorsKey]);
        Assert.Equal([FirstDescription, SecondDescription, ThirdDescription], errors);
    }

    [Fact]
    public void OnActionExecuted_ResultHasReferenceValue_ReplacesResultWithOkAndThatValue()
    {
        ErrorOr<string> value = SuccessValue;
        var context = BuildExecutedContext(new ObjectResult(value));

        _sut.OnActionExecuted(context);

        var ok = Assert.IsType<OkObjectResult>(context.Result);
        Assert.Equal(SuccessValue, ok.Value);
    }

    [Fact]
    public void OnActionExecuted_ResultHasValueTypeValue_ReplacesResultWithOkAndThatValue()
    {
        ErrorOr<int> value = NumberValue;
        var context = BuildExecutedContext(new ObjectResult(value));

        _sut.OnActionExecuted(context);

        var ok = Assert.IsType<OkObjectResult>(context.Result);
        Assert.Equal(NumberValue, ok.Value);
    }

    [Fact]
    public void OnActionExecuted_ResultIsSuccess_ReplacesResultWithNoContent()
    {
        ErrorOr<Success> value = Result.Success;
        var context = BuildExecutedContext(new ObjectResult(value));

        _sut.OnActionExecuted(context);

        Assert.IsType<NoContentResult>(context.Result);
    }

    [Fact]
    public void OnActionExecuted_ResultIsCreated_ReplacesResultWithNoContent()
    {
        ErrorOr<Created> value = Result.Created;
        var context = BuildExecutedContext(new ObjectResult(value));

        _sut.OnActionExecuted(context);

        Assert.IsType<NoContentResult>(context.Result);
    }

    [Fact]
    public void OnActionExecuted_ResultIsUpdated_ReplacesResultWithNoContent()
    {
        ErrorOr<Updated> value = Result.Updated;
        var context = BuildExecutedContext(new ObjectResult(value));

        _sut.OnActionExecuted(context);

        Assert.IsType<NoContentResult>(context.Result);
    }

    [Fact]
    public void OnActionExecuted_ResultIsDeleted_ReplacesResultWithNoContent()
    {
        ErrorOr<Deleted> value = Result.Deleted;
        var context = BuildExecutedContext(new ObjectResult(value));

        _sut.OnActionExecuted(context);

        Assert.IsType<NoContentResult>(context.Result);
    }

    [Fact]
    public void OnActionExecuted_ValueIsNotErrorOr_LeavesResultUnchanged()
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
