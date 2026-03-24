using FluentValidation;
using Polls.Application.Common.Constants;

namespace Polls.Application.Ideas.Commands.DeleteIdea;

public sealed class DeleteIdeaCommandValidator : AbstractValidator<DeleteIdeaCommand>
{
    public DeleteIdeaCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(ValidationConstants.Idea.IdRequired);
    }
}
