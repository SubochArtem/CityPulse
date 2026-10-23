using Polls.Domain.Common;
using Polls.Domain.Common.Enums;
using Xunit;

namespace Polls.Tests.Unit.Common.TestData;

public static class ErrorTestData
{
    public static TheoryData<Func<string, Error>, ErrorType> Factories => new()
    {
        { Error.NotFound, ErrorType.NotFound },
        { Error.Conflict, ErrorType.Conflict },
        { Error.Forbidden, ErrorType.Forbidden },
        { Error.Failure, ErrorType.Failure }
    };
}
