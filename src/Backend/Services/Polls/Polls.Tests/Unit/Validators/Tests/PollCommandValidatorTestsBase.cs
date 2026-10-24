using FluentValidation;
using FluentValidation.TestHelper;
using Polls.Application.Common.Constants;
using Polls.Tests.Unit.Validators.TestData;
using Xunit;

namespace Polls.Tests.Unit.Validators.Tests;

public abstract class PollCommandValidatorTestsBase<TCommand>
{
    private const string TitleProperty = "Title";
    private const string DescriptionProperty = "Description";
    private const string BudgetProperty = "BudgetAmount";
    private const string EndsAtProperty = "EndsAt";
    private const string DefaultTitle = "Budget for the new playground";
    private const string DefaultDescription = "Choose how to spend the city budget";
    private const decimal DefaultBudget = 1000;
    private const int ValidDurationDays = 10;
    private const int NearFutureMilliseconds = 300;
    private const int WaitMilliseconds = 600;

    protected abstract IValidator<TCommand> Validator { get; }

    protected abstract TCommand Build(string title, string? description, decimal budgetAmount, DateTimeOffset endsAt);

    protected static DateTimeOffset ValidEndsAt() => DateTimeOffset.UtcNow.AddDays(ValidDurationDays);

    private TCommand BuildValid() => Build(DefaultTitle, DefaultDescription, DefaultBudget, ValidEndsAt());

    [Theory]
    [MemberData(nameof(PollCommandValidatorTestData.EmptyTitles), MemberType = typeof(PollCommandValidatorTestData))]
    public void Validate_TitleIsEmpty_ShouldHaveValidationError(string? title)
    {
        var command = Build(title!, DefaultDescription, DefaultBudget, ValidEndsAt());

        var result = Validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(TitleProperty)
            .WithErrorMessage(ValidationConstants.Poll.TitleRequired);
    }

    [Theory]
    [MemberData(nameof(PollCommandValidatorTestData.TooLongTitles), MemberType = typeof(PollCommandValidatorTestData))]
    public void Validate_TitleIsTooLong_ShouldHaveValidationError(string title)
    {
        var command = Build(title, DefaultDescription, DefaultBudget, ValidEndsAt());

        var result = Validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(TitleProperty)
            .WithErrorMessage(ValidationConstants.Poll.TitleTooLong);
    }

    [Theory]
    [MemberData(nameof(PollCommandValidatorTestData.ValidTitles), MemberType = typeof(PollCommandValidatorTestData))]
    public void Validate_TitleIsValid_ShouldNotHaveValidationError(string title)
    {
        var command = Build(title, DefaultDescription, DefaultBudget, ValidEndsAt());

        var result = Validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(TitleProperty);
    }

    [Theory]
    [MemberData(nameof(PollCommandValidatorTestData.TooLongDescriptions), MemberType = typeof(PollCommandValidatorTestData))]
    public void Validate_DescriptionIsTooLong_ShouldHaveValidationError(string description)
    {
        var command = Build(DefaultTitle, description, DefaultBudget, ValidEndsAt());

        var result = Validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(DescriptionProperty)
            .WithErrorMessage(ValidationConstants.Poll.DescriptionTooLong);
    }

    [Theory]
    [MemberData(nameof(PollCommandValidatorTestData.ValidDescriptions), MemberType = typeof(PollCommandValidatorTestData))]
    public void Validate_DescriptionIsValid_ShouldNotHaveValidationError(string? description)
    {
        var command = Build(DefaultTitle, description, DefaultBudget, ValidEndsAt());

        var result = Validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(DescriptionProperty);
    }

    [Theory]
    [MemberData(nameof(PollCommandValidatorTestData.NonPositiveBudgets), MemberType = typeof(PollCommandValidatorTestData))]
    public void Validate_BudgetIsNotPositive_ShouldHaveValidationError(decimal budget)
    {
        var command = Build(DefaultTitle, DefaultDescription, budget, ValidEndsAt());

        var result = Validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(BudgetProperty)
            .WithErrorMessage(ValidationConstants.Poll.BudgetPositive);
    }

    [Theory]
    [MemberData(nameof(PollCommandValidatorTestData.TooHighBudgets), MemberType = typeof(PollCommandValidatorTestData))]
    public void Validate_BudgetIsTooHigh_ShouldHaveValidationError(decimal budget)
    {
        var command = Build(DefaultTitle, DefaultDescription, budget, ValidEndsAt());

        var result = Validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(BudgetProperty)
            .WithErrorMessage(ValidationConstants.Poll.BudgetTooHigh);
    }

    [Theory]
    [MemberData(nameof(PollCommandValidatorTestData.ValidBudgets), MemberType = typeof(PollCommandValidatorTestData))]
    public void Validate_BudgetIsValid_ShouldNotHaveValidationError(decimal budget)
    {
        var command = Build(DefaultTitle, DefaultDescription, budget, ValidEndsAt());

        var result = Validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(BudgetProperty);
    }

    [Theory]
    [MemberData(nameof(PollCommandValidatorTestData.PastEndDateOffsets), MemberType = typeof(PollCommandValidatorTestData))]
    public void Validate_EndsAtIsInThePast_ShouldHaveValidationError(TimeSpan offset)
    {
        var command = Build(DefaultTitle, DefaultDescription, DefaultBudget, DateTimeOffset.UtcNow.Add(offset));

        var result = Validator.TestValidate(command);

        Assert.Contains(
            result.Errors,
            error => error.PropertyName == EndsAtProperty
                     && error.ErrorMessage == ValidationConstants.Poll.EndDateInFuture);
    }

    [Fact]
    public async Task Validate_EndsAtPassesWhileValidatorIsAlive_ShouldHaveEndDateInFutureErrorOnNextValidation()
    {
        var command = Build(
            DefaultTitle,
            DefaultDescription,
            DefaultBudget,
            DateTimeOffset.UtcNow.AddMilliseconds(NearFutureMilliseconds));

        var before = Validator.TestValidate(command);
        await Task.Delay(WaitMilliseconds);
        var after = Validator.TestValidate(command);

        Assert.DoesNotContain(before.Errors, error => error.ErrorMessage == ValidationConstants.Poll.EndDateInFuture);
        Assert.Contains(after.Errors, error => error.ErrorMessage == ValidationConstants.Poll.EndDateInFuture);
    }

    [Fact]
    public void Validate_CommandIsValid_ShouldNotHaveAnyValidationErrors()
    {
        var result = Validator.TestValidate(BuildValid());

        result.ShouldNotHaveAnyValidationErrors();
    }
}
