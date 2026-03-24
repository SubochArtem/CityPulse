using FluentValidation;
using Polls.Application.Common.Constants;

namespace Polls.Application.Ideas.Queries.GetIdeas;

public sealed class GetIdeasQueryValidator : AbstractValidator<GetIdeasQuery>
{
    public GetIdeasQueryValidator()
    {
        RuleFor(x => x.Filter)
            .NotNull()
            .WithMessage(ValidationConstants.Pagination.FilterRequired)
            .SetValidator(new IdeaFilterValidator());
    }
}
