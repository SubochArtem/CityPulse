using FluentValidation;
using Users.Business.Constants;
using Users.Business.DTOs;

namespace Users.Business.Validators;

public class CreateUserValidator : AbstractValidator<CreateUserDto>
{
    private const string IdentityIdPattern =
        @"^[a-zA-Z0-9_-]+\|[a-zA-Z0-9@._-]+$";

    public CreateUserValidator()
    {
        RuleFor(x => x.IdentityId)
            .NotEmpty()
            .WithMessage(ValidationMessages.IdentityIdRequired)
            .Matches(IdentityIdPattern)
            .WithMessage(ValidationMessages.IdentityIdInvalidFormat);
    }
}
