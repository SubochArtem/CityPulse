using FluentValidation;
using Polls.Application.Common.Constants;

namespace Polls.Application.Polls.Commands.DeletePoll;

public sealed class DeletePollCommandValidator : AbstractValidator<DeletePollCommand>
{
    public DeletePollCommandValidator()
    {
        RuleFor(p => p.Id)
            .NotEmpty()
            .WithMessage(ValidationConstants.Poll.IdRequired);
    }
}
