using FluentValidation;
using FluentValidation.TestHelper;
using Polls.Application.Common.Constants;
using Polls.Tests.Unit.Validators.TestData;
using Xunit;

namespace Polls.Tests.Unit.Validators.Tests;

public abstract class IdeaCommandValidatorTestsBase<TCommand>
{
    private const string TitleProperty = "Title";
    private const string DescriptionProperty = "Description";
    private const string DefaultTitle = "Park renovation";
    private const string DefaultDescription = "Plant more trees in the central park";

    protected abstract IValidator<TCommand> Validator { get; }

    protected abstract TCommand Build(string title, string? description);

    [Theory]
    [MemberData(nameof(IdeaCommandValidatorTestData.EmptyTitles), MemberType = typeof(IdeaCommandValidatorTestData))]
    public void Validate_TitleIsEmpty_ShouldHaveValidationError(string? title)
    {
        var command = Build(title!, DefaultDescription);

        var result = Validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(TitleProperty)
            .WithErrorMessage(ValidationConstants.Idea.TitleRequired);
    }

    [Theory]
    [MemberData(nameof(IdeaCommandValidatorTestData.TooLongTitles), MemberType = typeof(IdeaCommandValidatorTestData))]
    public void Validate_TitleIsTooLong_ShouldHaveValidationError(string title)
    {
        var command = Build(title, DefaultDescription);

        var result = Validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(TitleProperty)
            .WithErrorMessage(ValidationConstants.Idea.TitleTooLong);
    }

    [Theory]
    [MemberData(nameof(IdeaCommandValidatorTestData.ValidTitles), MemberType = typeof(IdeaCommandValidatorTestData))]
    public void Validate_TitleIsValid_ShouldNotHaveValidationError(string title)
    {
        var command = Build(title, DefaultDescription);

        var result = Validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(TitleProperty);
    }

    [Theory]
    [MemberData(nameof(IdeaCommandValidatorTestData.TooLongDescriptions), MemberType = typeof(IdeaCommandValidatorTestData))]
    public void Validate_DescriptionIsTooLong_ShouldHaveValidationError(string description)
    {
        var command = Build(DefaultTitle, description);

        var result = Validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(DescriptionProperty)
            .WithErrorMessage(ValidationConstants.Idea.DescriptionTooLong);
    }

    [Theory]
    [MemberData(nameof(IdeaCommandValidatorTestData.ValidDescriptions), MemberType = typeof(IdeaCommandValidatorTestData))]
    public void Validate_DescriptionIsValid_ShouldNotHaveValidationError(string? description)
    {
        var command = Build(DefaultTitle, description);

        var result = Validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(DescriptionProperty);
    }

    [Fact]
    public void Validate_CommandIsValid_ShouldNotHaveAnyValidationErrors()
    {
        var result = Validator.TestValidate(Build(DefaultTitle, DefaultDescription));

        result.ShouldNotHaveAnyValidationErrors();
    }
}
