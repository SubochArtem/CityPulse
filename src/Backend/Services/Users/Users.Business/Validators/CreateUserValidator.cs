using FluentValidation;
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
            .WithMessage("IdentityId is required.")
            .Matches(IdentityIdPattern)
            .WithMessage("IdentityId must be in format '<provider>|<provider_user_id>'.");
    }
}
