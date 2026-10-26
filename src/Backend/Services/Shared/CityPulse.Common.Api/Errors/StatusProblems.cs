using Microsoft.AspNetCore.Http;

namespace CityPulse.Common.Api.Errors;

public static class StatusProblems
{
    private const string ValidationTitle = "Validation";
    private const string UnauthorizedTitle = "Unauthorized";
    private const string ForbiddenTitle = "Forbidden";
    private const string NotFoundTitle = "NotFound";
    private const string ConflictTitle = "Conflict";
    private const string FailureTitle = "Failure";

    private const string ValidationDetail = "The request is invalid.";
    private const string UnauthorizedDetail = "Authentication is required to access this resource.";
    private const string ForbiddenDetail = "You do not have permission to access this resource.";
    private const string NotFoundDetail = "The requested resource was not found.";
    private const string ConflictDetail = "The request conflicts with the current state of the resource.";
    private const string FailureDetail = "The request could not be completed.";

    public static (string Title, string Detail) Describe(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => (ValidationTitle, ValidationDetail),
        StatusCodes.Status401Unauthorized => (UnauthorizedTitle, UnauthorizedDetail),
        StatusCodes.Status403Forbidden => (ForbiddenTitle, ForbiddenDetail),
        StatusCodes.Status404NotFound => (NotFoundTitle, NotFoundDetail),
        StatusCodes.Status409Conflict => (ConflictTitle, ConflictDetail),
        _ => (FailureTitle, FailureDetail)
    };
}
