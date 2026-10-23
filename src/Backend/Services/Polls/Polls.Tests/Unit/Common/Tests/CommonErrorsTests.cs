using Polls.Domain.Common;
using Polls.Domain.Common.Enums;
using Xunit;

namespace Polls.Tests.Unit.Common.Tests;

public sealed class CommonErrorsTests
{
    [Fact]
    public void DatabaseError_Always_IsFailureWithDescription()
    {
        var error = CommonErrors.DatabaseError;

        Assert.Equal(ErrorType.Failure, error.Type);
        Assert.False(string.IsNullOrWhiteSpace(error.Description));
    }
}
