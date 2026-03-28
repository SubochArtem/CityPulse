using FluentValidation;
using Polls.Application.Common.Constants;

namespace Polls.Application.Polls.Commands.CreatePoll;

public sealed class CreatePollCommandValidator : AbstractValidator<CreatePollCommand>
{
    public CreatePollCommandValidator()
    {
        RuleFor(p => p.CityId)
            .NotEmpty()
            .WithMessage(ValidationConstants.City.IdRequired);

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
