using Feedback.API.Features.Feedback.EditComment;
using Feedback.Tests.Unit.Validators.TestData;
using FluentValidation.TestHelper;
using Xunit;

namespace Feedback.Tests.Unit.Validators.Tests;

public sealed class EditCommentCommandValidatorTests
{
    private const string TextProperty = "Text";
    private const string TextEmptyMessage = "Comment text must not be empty.";
    private const string TextTooLongMessage = "Comment text must not exceed 1000 characters.";

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid Id = Guid.NewGuid();

    private readonly EditCommentCommandValidator _validator = new();

    private static EditCommentCommand BuildCommand(string? text, bool bypassRestrictions = false) =>
        new(UserId, Id, text!, bypassRestrictions);

    [Fact]
    public void Validate_TextIsNull_ShouldHaveValidationError()
    {
        var result = _validator.TestValidate(BuildCommand(null));

        result.ShouldHaveValidationErrorFor(TextProperty)
            .WithErrorMessage(TextEmptyMessage);
    }

    [Theory]
    [MemberData(nameof(FeedbackTextTestData.EmptyTexts), MemberType = typeof(FeedbackTextTestData))]
    public void Validate_TextIsEmpty_ShouldHaveValidationError(string text)
    {
        var result = _validator.TestValidate(BuildCommand(text));

        result.ShouldHaveValidationErrorFor(TextProperty)
            .WithErrorMessage(TextEmptyMessage);
    }

    [Theory]
    [MemberData(nameof(FeedbackTextTestData.TooLongTexts), MemberType = typeof(FeedbackTextTestData))]
    public void Validate_TextIsTooLong_ShouldHaveValidationError(string text)
    {
        var result = _validator.TestValidate(BuildCommand(text));

        result.ShouldHaveValidationErrorFor(TextProperty)
            .WithErrorMessage(TextTooLongMessage);
    }

    [Theory]
    [MemberData(nameof(FeedbackTextTestData.ValidTexts), MemberType = typeof(FeedbackTextTestData))]
    public void Validate_TextIsValid_ShouldNotHaveValidationError(string text)
    {
        var result = _validator.TestValidate(BuildCommand(text));

        result.ShouldNotHaveValidationErrorFor(TextProperty);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Validate_CommandIsValid_ShouldNotHaveAnyValidationErrors(bool bypassRestrictions)
    {
        var result = _validator.TestValidate(BuildCommand("Edited comment", bypassRestrictions));

        result.ShouldNotHaveAnyValidationErrors();
    }
}
