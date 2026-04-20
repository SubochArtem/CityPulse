using FluentValidation;
using Polls.Application.Common.Constants;
using Polls.Application.Polls.Guards;

namespace Polls.Application.Polls.Commands.UpdatePoll;

public sealed class UpdatePollCommandValidator : AbstractValidator<UpdatePollCommand>
{
    public UpdatePollCommandValidator()
    {
        RuleFor(p => p.Id)
            .NotEmpty().WithMessage(ValidationConstants.Poll.IdRequired);

        RuleFor(p => p.Title)
            .ApplyTitleRules();
        
        RuleFor(p => p.Description)
            .ApplyDescriptionRules();
        
        RuleFor(p => p.BudgetAmount)
            .ApplyBudgetRules();

        RuleFor(p => p.EndsAt)
            .GreaterThan(DateTimeOffset.UtcNow).WithMessage(ValidationConstants.Poll.EndDateInFuture);
    }
}
