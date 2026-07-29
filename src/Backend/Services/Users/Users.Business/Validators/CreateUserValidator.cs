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
            .WithMessage(ValidationConstants.IdentityId.Required)
            .Matches(ValidationConstants.IdentityId.Pattern)
            .WithMessage(ValidationConstants.IdentityId.InvalidFormat);
    }
}
