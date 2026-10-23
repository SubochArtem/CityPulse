using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Feedback.API.Common.Filters;

public class ErrorOrResultFilter : IActionFilter
{
    private const string ErrorsExtensionKey = "errors";
    private const string ValuePropertyName = "Value";

    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Result is not ObjectResult { Value: IErrorOr errorOr })
            return;

        if (errorOr.IsError)
        {
            context.Result = CreateProblemResult(errorOr.Errors!, context.HttpContext.Request.Path);
            return;
        }

        var value = errorOr.GetType().GetProperty(ValuePropertyName)?.GetValue(errorOr);

        context.Result = value is null or Success or Created or Updated or Deleted
            ? new NoContentResult()
            : new OkObjectResult(value);
    }

    private static ObjectResult CreateProblemResult(List<Error> errors, PathString path)
    {
        var statusCode = GetStatusCode(errors[0].Type);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = errors[0].Type.ToString(),
            Detail = errors[0].Description,
            Instance = path
        };

        if (errors.Count > 1)
        {
            problemDetails.Extensions[ErrorsExtensionKey] = errors
                .Select(e => e.Description)
                .ToArray();
        }

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };
    }

    private static int GetStatusCode(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Failure => StatusCodes.Status503ServiceUnavailable,

        _ => StatusCodes.Status500InternalServerError
    };
}
