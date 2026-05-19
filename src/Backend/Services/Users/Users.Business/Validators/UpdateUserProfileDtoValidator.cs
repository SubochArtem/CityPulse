using FluentValidation;
using Users.Business.Constants;
using Users.Business.DTOs;

namespace Users.Business.Validators;

public class UpdateUserProfileDtoValidator : AbstractValidator<UpdateUserProfileDto>
{
    private const string NicknamePattern = "^[a-zA-Z0-9._+_-]+$";
    private const string NicknameConsecutivePattern = "^(?!.*[.+_-]{2})";

    public UpdateUserProfileDtoValidator()
    {
        RuleFor(x => x.Nickname)
            .NotEmpty()
            .WithMessage(ValidationMessages.NicknameRequired)
            .MinimumLength(3)
            .WithMessage(ValidationMessages.NicknameTooShort)
            .MaximumLength(30)
            .WithMessage(ValidationMessages.NicknameTooLong)
            .Matches(NicknamePattern)
            .WithMessage(ValidationMessages.NicknameInvalidCharacters)
            .Matches(NicknameConsecutivePattern)
            .WithMessage(ValidationMessages.NicknameConsecutiveSpecialCharacters);
    }
}
