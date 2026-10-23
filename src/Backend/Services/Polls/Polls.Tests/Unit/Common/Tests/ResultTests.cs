using Polls.Domain.Common;
using Polls.Domain.Common.Enums;
using Xunit;

namespace Polls.Tests.Unit.Common.Tests;

public sealed class ResultTests
{
    private const string SuccessValue = "value";
    private const string FirstErrorDescription = "first error";
    private const string SecondErrorDescription = "second error";

    private static readonly Error FirstError = Error.NotFound(FirstErrorDescription);
    private static readonly Error SecondError = Error.Conflict(SecondErrorDescription);

    [Fact]
    public void Success_WithValue_ReturnsSuccessfulResultWithValueAndNoErrors()
    {
        var result = Result<string>.Success(SuccessValue);

        Assert.True(result.IsSuccess);
        Assert.Equal(SuccessValue, result.Value);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Success_WithValue_ErrorIsUndefined()
    {
        var result = Result<string>.Success(SuccessValue);

        Assert.Equal(Error.Undefined, result.Error);
        Assert.Equal(ErrorType.Undefined, result.Error.Type);
    }

    [Fact]
    public void Success_WithNullReferenceValue_IsStillSuccessful()
    {
        var result = Result<string?>.Success(null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Failure_WithSingleError_ReturnsFailedResultWithThatError()
    {
        var result = Result<string>.Failure(FirstError);

        Assert.False(result.IsSuccess);
        Assert.Equal(FirstError, result.Error);
        Assert.Equal([FirstError], result.Errors);
    }

    [Fact]
    public void Failure_WithSingleError_ValueIsDefault()
    {
        var result = Result<string>.Failure(FirstError);

        Assert.Null(result.Value);
    }

    [Fact]
    public void Failure_WithValueTypeResult_ValueIsDefault()
    {
        var result = Result<int>.Failure(FirstError);

        Assert.Equal(0, result.Value);
    }

    [Fact]
    public void Failure_WithMultipleErrors_KeepsOrderAndExposesFirstAsError()
    {
        var result = Result<string>.Failure(FirstError, SecondError);

        Assert.False(result.IsSuccess);
        Assert.Equal([FirstError, SecondError], result.Errors);
        Assert.Equal(FirstError, result.Error);
    }

    [Fact]
    public void ImplicitConversion_FromValue_ReturnsSuccessfulResult()
    {
        Result<string> result = SuccessValue;

        Assert.True(result.IsSuccess);
        Assert.Equal(SuccessValue, result.Value);
    }

    [Fact]
    public void ImplicitConversion_FromError_ReturnsFailedResultWithThatError()
    {
        Result<string> result = FirstError;

        Assert.False(result.IsSuccess);
        Assert.Equal(FirstError, result.Error);
        Assert.Single(result.Errors);
    }
}
