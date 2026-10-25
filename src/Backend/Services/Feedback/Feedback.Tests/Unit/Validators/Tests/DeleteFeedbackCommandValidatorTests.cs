using Feedback.API.Features.Feedback.DeleteFeedback;
using FluentValidation.TestHelper;
using Xunit;

namespace Feedback.Tests.Unit.Validators.Tests;

public sealed class DeleteFeedbackCommandValidatorTests
{
    private const int ExpectedErrorsForEmptyIds = 2;

    private readonly DeleteFeedbackCommandValidator _validator = new();

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Validate_CommandIsValid_ShouldNotHaveAnyValidationErrors(bool bypassRestrictions)
    {
        var command = new DeleteFeedbackCommand(Guid.NewGuid(), Guid.NewGuid(), bypassRestrictions);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_BothIdsAreEmpty_ShouldHaveErrorForEachId()
    {
        var command = new DeleteFeedbackCommand(Guid.Empty, Guid.Empty, false);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
        Assert.Equal(ExpectedErrorsForEmptyIds, result.Errors.Count);
    }
}
