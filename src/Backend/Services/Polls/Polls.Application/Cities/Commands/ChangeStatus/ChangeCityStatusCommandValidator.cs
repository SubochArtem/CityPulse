using FluentValidation;
using Polls.Application.Common.Constants;

namespace Polls.Application.Cities.Commands.ChangeStatus;

public sealed class ChangeCityStatusCommandValidator : AbstractValidator<ChangeCityStatusCommand>
{
    public ChangeCityStatusCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty()
            .WithMessage(ValidationConstants.City.IdRequired);

        RuleFor(c => c.NewStatus)
            .IsInEnum()
            .WithMessage(ValidationConstants.City.InvalidStatus);
    }
}
