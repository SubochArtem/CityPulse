using FluentValidation;
using Polls.Application.Common.Constants;

namespace Polls.Application.Polls.Commands.UpdatePoll;

public sealed class UpdatePollCommandValidator : AbstractValidator<UpdatePollCommand>
{
    public UpdatePollCommandValidator()
    {
        RuleFor(p => p.Title)
            .NotEmpty()
            .WithMessage(ValidationConstants.Poll.TitleRequired)
            .MaximumLength(ValidationConstants.Poll.MaxTitleLength)
            .WithMessage(ValidationConstants.Poll.TitleTooLong);

        RuleFor(p => p.Description)
            .MaximumLength(ValidationConstants.Poll.MaxDescriptionLength)
            .WithMessage(ValidationConstants.Poll.DescriptionTooLong);

        RuleFor(p => p.BudgetAmount)
            .GreaterThan(0)
            .WithMessage(ValidationConstants.Poll.BudgetPositive)
            .LessThanOrEqualTo(ValidationConstants.Poll.MaxBudgetAmount)
            .WithMessage(ValidationConstants.Poll.BudgetTooHigh);

        RuleFor(p => p.EndsAt)
            .GreaterThan(DateTimeOffset.UtcNow)
            .WithMessage(ValidationConstants.Poll.EndDateInFuture);

        RuleFor(p => p.Id)
            .NotEmpty()
            .WithMessage(ValidationConstants.Poll.IdRequired);
    }
}
