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
            .WithMessage(ValidationConstants.User.IdentityIdRequired)
            .Matches(ValidationConstants.User.IdentityIdPattern)
            .WithMessage(ValidationConstants.User.IdentityIdInvalidFormat);
    }
}
