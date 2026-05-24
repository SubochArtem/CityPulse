using FluentValidation;
using Users.Business.Constants;
using Users.Business.DTOs;

namespace Users.Business.Validators;

public class CreateUserValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.IdentityId)
            .NotEmpty()
            .WithMessage(ValidationConstants.IdentityIdRequired)
            .Matches(ValidationConstants.IdentityIdPattern)
            .WithMessage(ValidationConstants.IdentityIdInvalidFormat);
    }
}
