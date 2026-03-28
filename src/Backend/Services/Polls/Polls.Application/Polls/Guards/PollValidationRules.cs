using FluentValidation;
using Polls.Application.Common.Constants;

namespace Polls.Application.Polls.Guards;

public static class PollValidationRules
{
    public static IRuleBuilderOptions<T, string> ApplyTitleRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage(ValidationConstants.Poll.TitleRequired)
            .MaximumLength(ValidationConstants.Poll.MaxTitleLength)
            .WithMessage(ValidationConstants.Poll.TitleTooLong);
    }

    public static IRuleBuilderOptions<T, string?> ApplyDescriptionRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(ValidationConstants.Poll.MaxDescriptionLength)
            .WithMessage(ValidationConstants.Poll.DescriptionTooLong);
    }

    public static IRuleBuilderOptions<T, decimal> ApplyBudgetRules<T>(this IRuleBuilder<T, decimal> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(ValidationConstants.Poll.MinBudgetAmount)
            .WithMessage(ValidationConstants.Poll.BudgetPositive)
            .LessThanOrEqualTo(ValidationConstants.Poll.MaxBudgetAmount)
            .WithMessage(ValidationConstants.Poll.BudgetTooHigh);
    }
}
