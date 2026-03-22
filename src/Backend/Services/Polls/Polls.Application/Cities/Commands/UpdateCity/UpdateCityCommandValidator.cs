using FluentValidation;
using Polls.Application.Cities.DTOs;
using Polls.Application.Common.Constants;

namespace Polls.Application.Cities.Commands.UpdateCity;

public sealed class UpdateCityCommandValidator : AbstractValidator<UpdateCityCommand>
{
    public UpdateCityCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(ValidationMessages.City.IdRequired);

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(ValidationMessages.City.TitleRequired)
            .MaximumLength(ValidationMessages.City.MaxTitleLength).WithMessage(ValidationMessages.City.TitleTooLong);

        RuleFor(x => x.Coordinates)
            .NotNull().WithMessage(ValidationMessages.City.CoordinatesRequired)
            .SetValidator(new CoordinatesDtoValidator());

        RuleFor(x => x.Description)
            .MaximumLength(ValidationMessages.City.MaxDescriptionLength)
            .WithMessage(ValidationMessages.City.DescriptionTooLong);
    }
}
