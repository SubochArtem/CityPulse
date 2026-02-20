using FluentValidation;
using Users.Business.DTOs;

namespace Users.Business.Validators;

public class UpdateUserValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.CityId)
            .NotEqual(Guid.Empty)
            .When(x => x.CityId.HasValue)
            .WithMessage("CityId must not be an empty GUID.");
    }
}
