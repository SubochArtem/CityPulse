using FluentValidation;
using Polls.Application.Common.Constants;

namespace Polls.Application.Ideas.Commands.ChangeApprovalStatus;

public sealed class ChangeIdeaApprovalStatusCommandValidator : AbstractValidator<ChangeIdeaApprovalStatusCommand>
{
    public ChangeIdeaApprovalStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(ValidationConstants.Idea.IdRequired);

        RuleFor(x => x.NewApprovalStatus)
            .IsInEnum()
            .WithMessage(ValidationConstants.Idea.InvalidApprovalStatus);
    }
}
