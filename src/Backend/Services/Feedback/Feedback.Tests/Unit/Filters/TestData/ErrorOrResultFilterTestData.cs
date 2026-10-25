using ErrorOr;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Feedback.Tests.Unit.Filters.TestData;

public static class ErrorOrResultFilterTestData
{
    private const int UnknownErrorType = 99;

    public static TheoryData<ErrorType, int> ErrorStatusMappings => new()
    {
        { ErrorType.Validation, StatusCodes.Status400BadRequest },
        { ErrorType.Unauthorized, StatusCodes.Status401Unauthorized },
        { ErrorType.Forbidden, StatusCodes.Status403Forbidden },
        { ErrorType.NotFound, StatusCodes.Status404NotFound },
        { ErrorType.Conflict, StatusCodes.Status409Conflict },
        { ErrorType.Failure, StatusCodes.Status503ServiceUnavailable },
        { ErrorType.Unexpected, StatusCodes.Status500InternalServerError },
        { (ErrorType)UnknownErrorType, StatusCodes.Status500InternalServerError }
    };
}
