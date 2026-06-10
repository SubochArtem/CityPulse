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
                .WithMessage(ValidationConstants.User.NicknameRequired)
                .MinimumLength(ValidationConstants.User.MinNicknameLength)
                .WithMessage(ValidationConstants.User.NicknameTooShort)
                .MaximumLength(ValidationConstants.User.MaxNicknameLength)
                .WithMessage(ValidationConstants.User.NicknameTooLong)
                .Matches(ValidationConstants.User.NicknamePattern)
                .WithMessage(ValidationConstants.User.NicknameInvalidCharacters)
                .Matches(ValidationConstants.User.NicknameConsecutivePattern)
                .WithMessage(ValidationConstants.User.NicknameConsecutiveSpecialCharacters);
        });
    }
}
