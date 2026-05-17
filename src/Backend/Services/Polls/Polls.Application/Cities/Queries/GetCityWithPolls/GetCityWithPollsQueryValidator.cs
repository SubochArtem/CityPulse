using FluentValidation;
using Polls.Application.Common.Constants;

namespace Polls.Application.Cities.Queries.GetCityWithPolls;

public sealed class GetCityWithPollsQueryValidator : AbstractValidator<GetCityWithPollsQuery>
{
    public GetCityWithPollsQueryValidator()
    {
        RuleFor(q => q.Id)
            .NotEmpty()
            .WithMessage(ValidationConstants.City.IdRequired);
    }
}
