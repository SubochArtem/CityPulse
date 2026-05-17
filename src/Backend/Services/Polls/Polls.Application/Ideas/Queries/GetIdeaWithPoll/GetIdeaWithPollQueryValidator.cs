using FluentValidation;
using Polls.Application.Common.Constants;

namespace Polls.Application.Ideas.Queries.GetIdeaWithPoll;

public sealed class GetIdeaWithPollQueryValidator : AbstractValidator<GetIdeaWithPollQuery>
{
    public GetIdeaWithPollQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(ValidationConstants.Idea.IdRequired);
    }
}
