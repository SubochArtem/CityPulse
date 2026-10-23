using FluentValidation;
using Polls.Application.Common.Constants;

namespace Polls.Application.Polls.Commands.ChangeStatus;

public class ChangePollStatusCommandValidator : AbstractValidator<ChangePollStatusCommand>
{
    public ChangePollStatusCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty()
            .WithMessage(ValidationConstants.Poll.IdRequired);

        RuleFor(c => c.NewStatus)
            .IsInEnum()
            .WithMessage(ValidationConstants.Poll.InvalidStatus);
    }
}
