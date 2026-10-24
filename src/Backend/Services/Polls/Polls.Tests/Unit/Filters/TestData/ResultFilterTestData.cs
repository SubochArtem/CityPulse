using Microsoft.AspNetCore.Http;
using Polls.Domain.Common.Enums;
using Xunit;

namespace Polls.Tests.Unit.Filters.TestData;

public static class ResultFilterTestData
{
    public static TheoryData<ErrorType, int> ErrorStatusMappings => new()
    {
        { ErrorType.NotFound, StatusCodes.Status404NotFound },
        { ErrorType.Conflict, StatusCodes.Status409Conflict },
        { ErrorType.Forbidden, StatusCodes.Status403Forbidden },
        { ErrorType.Validation, StatusCodes.Status400BadRequest },
        { ErrorType.Failure, StatusCodes.Status500InternalServerError },
        { ErrorType.Undefined, StatusCodes.Status400BadRequest },
        { (ErrorType)99, StatusCodes.Status400BadRequest }
    };
}
