using FluentValidation;
using Users.Business.Constants;
using Users.Business.DTOs;

namespace Users.Business.Validators;

public class PublicProfilesRequestDtoValidator : AbstractValidator<PublicProfilesRequestDto>
{
    public PublicProfilesRequestDtoValidator()
    {
        RuleFor(x => x.Ids)
            .NotEmpty()
            .WithMessage(ValidationConstants.PublicProfiles.IdsRequired)
            .Must(ids => ids.Length <= ValidationConstants.PublicProfiles.MaxIds)
            .WithMessage(ValidationConstants.PublicProfiles.TooManyIds);
    }
}
