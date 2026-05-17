using FluentValidation;
using Polls.Application.Common.Constants;
using Polls.Application.Common.Models;

namespace Polls.Application.Common.Validators;

public abstract class BaseFilterValidator<T> : AbstractValidator<T> where T : BaseFilter
{
    protected BaseFilterValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(ValidationConstants.Pagination.MinPage)
            .WithMessage(ValidationConstants.Pagination.PageInvalid);

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(ValidationConstants.Pagination.MinPageSize)
            .WithMessage(ValidationConstants.Pagination.PageSizeTooSmall)
            .LessThanOrEqualTo(ValidationConstants.Pagination.MaxPageSize)
            .WithMessage(ValidationConstants.Pagination.PageSizeTooLarge);

        RuleFor(x => x.SearchTerm)
            .MaximumLength(ValidationConstants.Search.MaxSearchTermLength)
            .WithMessage(ValidationConstants.Search.SearchTermTooLong)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm));
    }
}
