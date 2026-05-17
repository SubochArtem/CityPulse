using FluentValidation;
using Polls.Application.Common.Constants;

namespace Polls.Application.Cities.Queries.GetCities;

public sealed class GetCitiesQueryValidator : AbstractValidator<GetCitiesQuery>
{
    public GetCitiesQueryValidator()
    {
        RuleFor(x => x.Filter)
            .NotNull()
            .WithMessage(ValidationConstants.Pagination.FilterRequired)
            .SetValidator(new CityFilterValidator());
    }
}
