using FluentValidation;
using Polls.Application.Cities.DTOs;
using Polls.Application.Common.Constants;

namespace Polls.Application.Cities.Commands.UpdateCity;

public sealed class UpdateCityCommandValidator : AbstractValidator<UpdateCityCommand>
{
    public UpdateCityCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage(ValidationConstants.City.IdRequired);

        RuleFor(c => c.Title)
            .NotEmpty().WithMessage(ValidationConstants.City.TitleRequired)
            .MaximumLength(ValidationConstants.City.MaxTitleLength).WithMessage(ValidationConstants.City.TitleTooLong);

        RuleFor(c => c.Coordinates)
            .NotNull().WithMessage(ValidationConstants.City.CoordinatesRequired)
            .SetValidator(new CoordinatesDtoValidator());

        RuleFor(c => c.Description)
            .MaximumLength(ValidationConstants.City.MaxDescriptionLength)
            .WithMessage(ValidationConstants.City.DescriptionTooLong);
    }
}
