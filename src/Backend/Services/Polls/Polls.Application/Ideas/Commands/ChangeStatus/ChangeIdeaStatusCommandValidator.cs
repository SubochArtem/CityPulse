using FluentValidation;
using Polls.Application.Common.Constants;

namespace Polls.Application.Ideas.Commands.ChangeStatus;

public class ChangeIdeaStatusCommandValidator : AbstractValidator<ChangeIdeaStatusCommand>
{
    public ChangeIdeaStatusCommandValidator()
    {
        RuleFor(i => i.Id)
            .NotEmpty()
            .WithMessage(ValidationConstants.Idea.IdRequired);

        RuleFor(i => i.NewStatus)
            .IsInEnum()
            .WithMessage(ValidationConstants.Idea.InvalidStatus);
    }
}
