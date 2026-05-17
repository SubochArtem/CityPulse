using FluentValidation;
using Polls.Application.Common.Constants;

namespace Polls.Application.Polls.Queries.GetPollById;

public sealed class GetPollByIdQueryValidator : AbstractValidator<GetPollByIdQuery>
{
    public GetPollByIdQueryValidator()
    {
        RuleFor(q => q.Id)
            .NotEmpty()
            .WithMessage(ValidationConstants.Poll.IdRequired);
    }
}
