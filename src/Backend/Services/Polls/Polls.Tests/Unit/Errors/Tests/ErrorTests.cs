using Polls.Domain.Common;
using Polls.Domain.Common.Enums;
using Polls.Tests.Unit.Errors.TestData;
using Xunit;

namespace Polls.Tests.Unit.Errors.Tests;

public sealed class ErrorTests
{
    private const string Description = "something went wrong";

    [Theory]
    [MemberData(nameof(ErrorTestData.Factories), MemberType = typeof(ErrorTestData))]
    public void Factory_ValidDescription_CreatesErrorWithExpectedTypeAndDescription(
        Func<string, Error> factory,
        ErrorType expectedType)
    {
        var error = factory(Description);

        Assert.Equal(expectedType, error.Type);
        Assert.Equal(Description, error.Description);
    }

    [Fact]
    public void Undefined_Always_HasUndefinedTypeAndEmptyDescription()
    {
        var error = Error.Undefined;

        Assert.Equal(ErrorType.Undefined, error.Type);
        Assert.Equal(string.Empty, error.Description);
    }

    [Fact]
    public void Equals_SameTypeAndDescription_ReturnsTrue()
    {
        var first = Error.Conflict(Description);
        var second = Error.Conflict(Description);

        Assert.Equal(first, second);
    }

    [Fact]
    public void Equals_SameDescriptionButDifferentType_ReturnsFalse()
    {
        var conflict = Error.Conflict(Description);
        var forbidden = Error.Forbidden(Description);

        Assert.NotEqual(conflict, forbidden);
    }
}
