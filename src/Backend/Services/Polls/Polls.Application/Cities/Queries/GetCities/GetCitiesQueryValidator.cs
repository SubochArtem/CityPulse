using FluentValidation;
using Polls.Application.Common.Constants;

namespace Polls.Application.Cities.Queries.GetCities;

public sealed class GetCitiesQueryValidator : AbstractValidator<GetCitiesQuery>
{
    public GetCitiesQueryValidator()
    {
        RuleFor(x => x.Filter)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage(ValidationConstants.Pagination.FilterRequired);

        RuleFor(x => x.Filter.Page)
            .GreaterThanOrEqualTo(ValidationConstants.Pagination.MinPage)
            .WithMessage(ValidationConstants.Pagination.PageInvalid);

        RuleFor(x => x.Filter.PageSize)
            .GreaterThanOrEqualTo(ValidationConstants.Pagination.MinPageSize)
            .WithMessage(ValidationConstants.Pagination.PageSizeTooSmall)
            .LessThanOrEqualTo(ValidationConstants.Pagination.MaxPageSize)
            .WithMessage(ValidationConstants.Pagination.PageSizeTooLarge);

        RuleFor(x => x.Filter.Status)
            .IsInEnum()
            .When(x => x.Filter.Status.HasValue)
            .WithMessage(ValidationConstants.City.InvalidStatus);

        RuleFor(x => x.Filter.SearchTerm)
            .MaximumLength(ValidationConstants.City.MaxSearchTermLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Filter.SearchTerm))
            .WithMessage(ValidationConstants.City.SearchTermTooLong);
    }
}
