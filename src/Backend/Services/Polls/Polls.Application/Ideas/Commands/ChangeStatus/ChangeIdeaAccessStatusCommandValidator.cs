using FluentValidation;
using Polls.Application.Common.Constants;

namespace Polls.Application.Ideas.Commands.ChangeStatus;

public class ChangeIdeaAccessStatusCommandValidator : AbstractValidator<ChangeIdeaAccessStatusCommand>
{
    public ChangeIdeaAccessStatusCommandValidator()
    {
        RuleFor(i => i.Id)
            .NotEmpty()
            .WithMessage(ValidationConstants.Idea.IdRequired);

        RuleFor(i => i.NewAccessStatus)
            .IsInEnum()
            .WithMessage(ValidationConstants.Idea.InvalidAccessStatus);
    }
}
