using FluentValidation;
using Polls.Application.Common.Constants;

namespace Polls.Application.Cities.Queries.GetCityById;

public sealed class GetCityByIdQueryValidator : AbstractValidator<GetCityByIdQuery>
{
    public GetCityByIdQueryValidator()
    {
        RuleFor(q => q.Id)
            .NotEmpty()
            .WithMessage(ValidationConstants.City.IdRequired);
    }
}
