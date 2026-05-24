using FluentValidation;
using Users.Business.Constants;
using Users.Business.DTOs;

namespace Users.Business.Validators;

public class UpdateUserProfileDtoValidator : AbstractValidator<UpdateUserProfileDto>
{

    public UpdateUserProfileDtoValidator()
    {
        RuleFor(x => x.Nickname)
            .NotEmpty()
            .WithMessage(ValidationConstants.NicknameRequired)
            .MinimumLength(ValidationConstants.MinNicknameLength)
            .WithMessage(ValidationConstants.NicknameTooShort)
            .MaximumLength(ValidationConstants.MaxNicknameLength)
            .WithMessage(ValidationConstants.NicknameTooLong)
            .Matches(ValidationConstants.NicknamePattern)
            .WithMessage(ValidationConstants.NicknameInvalidCharacters)
            .Matches(ValidationConstants.NicknameConsecutivePattern)
            .WithMessage(ValidationConstants.NicknameConsecutiveSpecialCharacters);
    }
}
