using FluentValidation.TestHelper;
using Polls.Application.Cities.DTOs;
using Polls.Application.Common.Constants;
using Polls.Tests.Unit.Validators.TestData;
using Xunit;

namespace Polls.Tests.Unit.Validators.Tests;

public sealed class CoordinatesDtoValidatorTests
{
    private const double DefaultLatitude = 55.75;
    private const double DefaultLongitude = 37.62;

    private readonly CoordinatesDtoValidator _validator = new();

    [Theory]
    [MemberData(nameof(CoordinatesDtoValidatorTestData.InvalidLatitudes), MemberType = typeof(CoordinatesDtoValidatorTestData))]
    public void Validate_LatitudeIsOutOfRange_ShouldHaveValidationError(double latitude)
    {
        var dto = new CoordinatesDto(latitude, DefaultLongitude);

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Latitude)
            .WithErrorMessage(ValidationConstants.Coordinates.InvalidLatitude);
        result.ShouldNotHaveValidationErrorFor(x => x.Longitude);
    }

    [Theory]
    [MemberData(nameof(CoordinatesDtoValidatorTestData.ValidLatitudes), MemberType = typeof(CoordinatesDtoValidatorTestData))]
    public void Validate_LatitudeIsInRange_ShouldNotHaveValidationError(double latitude)
    {
        var dto = new CoordinatesDto(latitude, DefaultLongitude);

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Latitude);
    }

    [Theory]
    [MemberData(nameof(CoordinatesDtoValidatorTestData.InvalidLongitudes), MemberType = typeof(CoordinatesDtoValidatorTestData))]
    public void Validate_LongitudeIsOutOfRange_ShouldHaveValidationError(double longitude)
    {
        var dto = new CoordinatesDto(DefaultLatitude, longitude);

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Longitude)
            .WithErrorMessage(ValidationConstants.Coordinates.InvalidLongitude);
        result.ShouldNotHaveValidationErrorFor(x => x.Latitude);
    }

    [Theory]
    [MemberData(nameof(CoordinatesDtoValidatorTestData.ValidLongitudes), MemberType = typeof(CoordinatesDtoValidatorTestData))]
    public void Validate_LongitudeIsInRange_ShouldNotHaveValidationError(double longitude)
    {
        var dto = new CoordinatesDto(DefaultLatitude, longitude);

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Longitude);
    }

    [Fact]
    public void Validate_BothCoordinatesAreOutOfRange_ShouldHaveErrorsForBoth()
    {
        var dto = new CoordinatesDto(double.NaN, double.NaN);

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Latitude);
        result.ShouldHaveValidationErrorFor(x => x.Longitude);
    }

    [Fact]
    public void Validate_CoordinatesAreValid_ShouldNotHaveAnyValidationErrors()
    {
        var dto = new CoordinatesDto(DefaultLatitude, DefaultLongitude);

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
