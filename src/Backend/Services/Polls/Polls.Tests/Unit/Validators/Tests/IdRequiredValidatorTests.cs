using FluentValidation;
using Polls.Tests.Unit.Validators.TestData;
using Xunit;

namespace Polls.Tests.Unit.Validators.Tests;

public sealed class IdRequiredValidatorTests
{
    private const string IdProperty = "Id";

    [Theory]
    [MemberData(nameof(IdRequiredValidatorTestData.Cases), MemberType = typeof(IdRequiredValidatorTestData))]
    public void Validate_IdIsEmpty_ShouldHaveValidationError(IdRequiredCase testCase)
    {
        var command = testCase.BuildCommand(Guid.Empty);

        var result = testCase.Validator.Validate(new ValidationContext<object>(command));

        var error = Assert.Single(result.Errors);
        Assert.Equal(IdProperty, error.PropertyName);
        Assert.Equal(testCase.ExpectedMessage, error.ErrorMessage);
    }

    [Theory]
    [MemberData(nameof(IdRequiredValidatorTestData.Cases), MemberType = typeof(IdRequiredValidatorTestData))]
    public void Validate_IdIsProvided_ShouldNotHaveValidationError(IdRequiredCase testCase)
    {
        var command = testCase.BuildCommand(Guid.NewGuid());

        var result = testCase.Validator.Validate(new ValidationContext<object>(command));

        Assert.True(result.IsValid);
    }
}
