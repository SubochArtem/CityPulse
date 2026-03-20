using FluentValidation;
using Polls.Application.Common.Constants;

namespace Polls.Application.Cities.DTOs;

public sealed class CoordinatesDtoValidator : AbstractValidator<CoordinatesDto>
{
    public CoordinatesDtoValidator()
    {
        RuleFor(x => x.Latitude)
            .InclusiveBetween(
                ValidationMessages.Coordinates.MinLatitude,
                ValidationMessages.Coordinates.MaxLatitude)
            .WithMessage(ValidationMessages.Coordinates.InvalidLatitude);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(
                ValidationMessages.Coordinates.MinLongitude,
                ValidationMessages.Coordinates.MaxLongitude)
            .WithMessage(ValidationMessages.Coordinates.InvalidLongitude);
    }
}
