using FluentValidation;
using Users.Business.Constants;
using Users.Business.DTOs;

namespace Users.Business.Validators;

public class UserFilterDtoValidator : AbstractValidator<UserFilterDto>
{
    public UserFilterDtoValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(ValidationConstants.Pagination.MinPage)
            .WithMessage(ValidationConstants.Pagination.PageInvalid);

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(ValidationConstants.Pagination.MinPageSize)
            .WithMessage(ValidationConstants.Pagination.PageSizeTooSmall)
            .LessThanOrEqualTo(ValidationConstants.Pagination.MaxPageSize)
            .WithMessage(ValidationConstants.Pagination.PageSizeTooLarge);
    }
}
