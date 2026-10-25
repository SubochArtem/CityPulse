using Feedback.Tests.Unit.Validators.TestData;
using FluentValidation;
using Xunit;

namespace Feedback.Tests.Unit.Validators.Tests;

public sealed class RequiredGuidValidatorTests
{
    [Theory]
    [MemberData(nameof(RequiredGuidValidatorTestData.Cases), MemberType = typeof(RequiredGuidValidatorTestData))]
    public void Validate_GuidIsEmpty_ShouldHaveValidationError(RequiredGuidCase testCase)
    {
        var command = testCase.BuildCommand(Guid.Empty);

        var result = testCase.Validator.Validate(new ValidationContext<object>(command));

        var error = Assert.Single(result.Errors);
        Assert.Equal(testCase.PropertyName, error.PropertyName);
        Assert.Equal(testCase.ExpectedMessage, error.ErrorMessage);
    }

    [Theory]
    [MemberData(nameof(RequiredGuidValidatorTestData.Cases), MemberType = typeof(RequiredGuidValidatorTestData))]
    public void Validate_GuidIsProvided_ShouldNotHaveValidationError(RequiredGuidCase testCase)
    {
        var command = testCase.BuildCommand(Guid.NewGuid());

        var result = testCase.Validator.Validate(new ValidationContext<object>(command));

        Assert.True(result.IsValid);
    }
}
