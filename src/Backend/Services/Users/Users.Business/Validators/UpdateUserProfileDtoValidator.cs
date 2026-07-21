using FluentValidation;
using Users.Business.Constants;
using Users.Business.DTOs;

namespace Users.Business.Validators;

public class UpdateUserProfileDtoValidator : AbstractValidator<UpdateUserProfileDto>
{
    public UpdateUserProfileDtoValidator()
    {
        When(x => x.Nickname is not null, () =>
        {
            RuleFor(x => x.Nickname)
                .NotEmpty() 
                .WithMessage(ValidationConstants.Nickname.Required)
                
                .MinimumLength(ValidationConstants.Nickname.MinLength)
                .WithMessage(ValidationConstants.Nickname.TooShort)
                
                .MaximumLength(ValidationConstants.Nickname.MaxLength)
                .WithMessage(ValidationConstants.Nickname.TooLong)
                
                .Matches(ValidationConstants.Nickname.Pattern)
                .WithMessage(ValidationConstants.Nickname.InvalidCharacters)
                
                .Matches(ValidationConstants.Nickname.ConsecutivePattern)
                .WithMessage(ValidationConstants.Nickname.ConsecutiveSpecialCharacters);
        });
    }
}
