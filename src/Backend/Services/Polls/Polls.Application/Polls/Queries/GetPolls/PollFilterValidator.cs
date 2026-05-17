using FluentValidation;
using Polls.Application.Common.Constants;
using Polls.Application.Common.Models;
using Polls.Application.Common.Validators;

namespace Polls.Application.Polls.Queries.GetPolls;

public sealed class PollFilterValidator : BaseFilterValidator<PollFilter>
{
    public PollFilterValidator()
    {
        RuleFor(x => x.CityId)
            .NotEmpty()
            .WithMessage(ValidationConstants.City.IdRequired)
            .When(x => x.CityId.HasValue);

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage(ValidationConstants.Poll.InvalidType)
            .When(x => x.Type.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage(ValidationConstants.Poll.InvalidStatus)
            .When(x => x.Status.HasValue);
    }
}
