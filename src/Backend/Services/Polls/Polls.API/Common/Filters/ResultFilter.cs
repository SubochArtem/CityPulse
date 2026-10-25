using CityPulse.Common.Api.Errors;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Polls.Domain.Common;
using Polls.Domain.Common.Enums;

namespace Polls.API.Common.Filters;

public class ResultFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Result is not ObjectResult { Value: Result result })
            return;

        if (result.IsSuccess)
        {
            UnwrapSuccess(context, result);
            return;
        }

        var statusCode = GetStatusCode(result.Error.Type);

        var problemDetails = ProblemDetailsFactory.Create(
            statusCode,
            result.Error.Type.ToString(),
            result.Error.Description,
            context.HttpContext.Request.Path,
            result.Errors.Select(e => e.Description).ToArray());

        context.Result = new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };
    }

    private static void UnwrapSuccess(ActionExecutedContext context, Result result)
    {
        if (result is not IResultWithValue resultWithValue)
            return;

        context.Result = resultWithValue.GetValue() switch
        {
            null or Unit => new NoContentResult(),
            var value => new OkObjectResult(value)
        };
    }

    private static int GetStatusCode(ErrorType type) => type switch
    {
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Failure => StatusCodes.Status500InternalServerError,
        
        _=> StatusCodes.Status400BadRequest
    };
}
