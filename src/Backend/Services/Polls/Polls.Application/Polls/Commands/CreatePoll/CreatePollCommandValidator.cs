using FluentValidation;
using Polls.Application.Common.Constants;
using Polls.Application.Polls.Guards;

namespace Polls.Application.Polls.Commands.CreatePoll;

public sealed class CreatePollCommandValidator : AbstractValidator<CreatePollCommand>
{
    public CreatePollCommandValidator()
    {
        RuleFor(p => p.CityId)
            .NotEmpty()
            .WithMessage(ValidationConstants.City.IdRequired);

        RuleFor(p => p.Title)
            .ApplyTitleRules();
        RuleFor(p => p.Description)
            .ApplyDescriptionRules();
        RuleFor(p => p.BudgetAmount)
            .ApplyBudgetRules();

        RuleFor(p => p.EndsAt)
            .GreaterThan(DateTimeOffset.UtcNow)
            .WithMessage(ValidationConstants.Poll.EndDateInFuture)
            .Must(BeAtLeastMinDuration)
            .WithMessage(ValidationConstants.Poll.TooShortDuration)
            .Must(BeWithinMaxDuration)
            .WithMessage(ValidationConstants.Poll.TooLongDuration);

        RuleFor(p => p.Type)
            .IsInEnum().WithMessage(ValidationConstants.Poll.InvalidType);
    }

    private static bool BeAtLeastMinDuration(DateTimeOffset endsAt)
    {
        return endsAt >= DateTimeOffset.UtcNow.AddDays(ValidationConstants.Poll.MinDurationDays);
    }

    private static bool BeWithinMaxDuration(DateTimeOffset endsAt)
    {
        return endsAt <= DateTimeOffset.UtcNow.AddDays(ValidationConstants.Poll.MaxDurationDays);
    }
}
