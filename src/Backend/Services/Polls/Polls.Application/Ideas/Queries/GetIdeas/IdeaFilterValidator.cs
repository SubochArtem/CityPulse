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

        RuleFor(x => x.AccessStatus)
            .IsInEnum()
            .WithMessage(ValidationConstants.Idea.InvalidAccessStatus)
            .When(x => x.AccessStatus.HasValue);
        
        RuleFor(x => x.ApprovalStatus)
            .IsInEnum()
            .WithMessage(ValidationConstants.Idea.InvalidApprovalStatus)
            .When(x => x.ApprovalStatus.HasValue);
    }
}
