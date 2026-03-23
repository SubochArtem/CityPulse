using FluentValidation;
using Polls.Application.Common.Constants;

namespace Polls.Application.Polls.Queries.GetPolls;

public sealed class GetPollsQueryValidator : AbstractValidator<GetPollsQuery>
{
    public GetPollsQueryValidator()
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

        RuleFor(x => x.Filter.CityId)
            .NotEmpty()
            .When(x => x.Filter.CityId.HasValue)
            .WithMessage(ValidationConstants.City.IdRequired);
    }
}
