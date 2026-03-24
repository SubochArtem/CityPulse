using FluentValidation;
using Polls.Application.Common.Constants;

namespace Polls.Application.Polls.Queries.GetPolls;

public sealed class GetPollsQueryValidator : AbstractValidator<GetPollsQuery>
{
    public GetPollsQueryValidator()
    {
        RuleFor(x => x.Filter)
            .NotNull()
            .WithMessage(ValidationConstants.Pagination.FilterRequired)
            .SetValidator(new PollFilterValidator());
    }
}
