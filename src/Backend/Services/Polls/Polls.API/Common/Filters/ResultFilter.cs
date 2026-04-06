using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Polls.Domain.Common;
using Polls.Domain.Common.Enums;

namespace Polls.API.Common.Filters;

public class ResultFilter : IActionFilter
{
    private const string ErrorsExtensionKey = "errors";
    
    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Result is not ObjectResult { Value: Result result })
            return;

        if (result.IsSuccess)
            return;

        var statusCode = GetStatusCode(result.Error.Type);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = result.Error.Type.ToString(),
            Detail = result.Error.Description,
            Instance = context.HttpContext.Request.Path
        };

        if (result.Errors.Count > 1)
            problemDetails.Extensions[ErrorsExtensionKey] = result.Errors
                .Select(e => e.Description)
                .ToArray();

        context.Result = new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };
    }

    private static int GetStatusCode(ErrorType type) => type switch
    {
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        _=> StatusCodes.Status400BadRequest
    };
}
