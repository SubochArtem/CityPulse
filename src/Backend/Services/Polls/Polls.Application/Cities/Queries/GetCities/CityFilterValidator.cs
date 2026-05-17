using FluentValidation;
using Polls.Application.Common.Constants;
using Polls.Application.Common.Models;
using Polls.Application.Common.Validators;

namespace Polls.Application.Cities.Queries.GetCities;

public sealed class CityFilterValidator : BaseFilterValidator<CityFilter>
{
    public CityFilterValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage(ValidationConstants.City.InvalidStatus)
            .When(x => x.Status.HasValue);
    }
}
