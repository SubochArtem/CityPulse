using FluentValidation;
using Polls.Application.Common.Constants;

namespace Polls.Application.Cities.DTOs;

public sealed class CoordinatesDtoValidator : AbstractValidator<CoordinatesDto>
{
    public CoordinatesDtoValidator()
    {
        RuleFor(x => x.Latitude)
            .InclusiveBetween(
                ValidationConstants.Coordinates.MinLatitude,
                ValidationConstants.Coordinates.MaxLatitude)
            .WithMessage(ValidationConstants.Coordinates.InvalidLatitude);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(
                ValidationConstants.Coordinates.MinLongitude,
                ValidationConstants.Coordinates.MaxLongitude)
            .WithMessage(ValidationConstants.Coordinates.InvalidLongitude);
    }
}