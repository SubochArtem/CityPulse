using FluentValidation;
using Polls.Application.Common.Constants;
using Polls.Application.Common.Models;
using Polls.Application.Common.Validators;

namespace Polls.Application.Ideas.Queries.GetIdeas;

public sealed class IdeaFilterValidator : BaseFilterValidator<IdeaFilter>
{
    public IdeaFilterValidator()
    {
        RuleFor(x => x.PollId)
            .NotEmpty()
            .WithMessage(ValidationConstants.Poll.IdRequired);

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage(ValidationConstants.Idea.InvalidStatus)
            .When(x => x.Status.HasValue);
    }
}
