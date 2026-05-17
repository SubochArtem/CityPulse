using FluentValidation;
using Polls.Application.Common.Constants;

namespace Polls.Application.Polls.Queries.GetPollWithIdeas;

public class GetPollWithIdeasQueryValidator : AbstractValidator<GetPollWithIdeasQuery>
{
    public GetPollWithIdeasQueryValidator()
    {
        RuleFor(q => q.Id)
            .NotEmpty()
            .WithMessage(ValidationConstants.Poll.IdRequired);
    }
}
