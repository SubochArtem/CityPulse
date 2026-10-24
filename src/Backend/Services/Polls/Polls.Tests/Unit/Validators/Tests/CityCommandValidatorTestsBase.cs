using FluentValidation;
using FluentValidation.TestHelper;
using Polls.Application.Cities.DTOs;
using Polls.Application.Common.Constants;
using Polls.Tests.Unit.Validators.TestData;
using Xunit;

namespace Polls.Tests.Unit.Validators.Tests;

public abstract class CityCommandValidatorTestsBase<TCommand>
{
    private const string TitleProperty = "Title";
    private const string CoordinatesProperty = "Coordinates";
    private const string LatitudeProperty = "Coordinates.Latitude";
    private const string DescriptionProperty = "Description";
    private const string DefaultTitle = "Warsaw";
    private const string DefaultDescription = "Capital of Poland";
    private const double InvalidLatitude = 91;

    protected abstract IValidator<TCommand> Validator { get; }

    protected abstract TCommand Build(string title, CoordinatesDto? coordinates, string? description);

    protected static CoordinatesDto ValidCoordinates() => new(52.23, 21.01);

    private TCommand BuildValid() => Build(DefaultTitle, ValidCoordinates(), DefaultDescription);

    [Theory]
    [MemberData(nameof(CityCommandValidatorTestData.EmptyTitles), MemberType = typeof(CityCommandValidatorTestData))]
    public void Validate_TitleIsEmpty_ShouldHaveValidationError(string? title)
    {
        var command = Build(title!, ValidCoordinates(), DefaultDescription);

        var result = Validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(TitleProperty)
            .WithErrorMessage(ValidationConstants.City.TitleRequired);
    }

    [Theory]
    [MemberData(nameof(CityCommandValidatorTestData.TooLongTitles), MemberType = typeof(CityCommandValidatorTestData))]
    public void Validate_TitleIsTooLong_ShouldHaveValidationError(string title)
    {
        var command = Build(title, ValidCoordinates(), DefaultDescription);

        var result = Validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(TitleProperty)
            .WithErrorMessage(ValidationConstants.City.TitleTooLong);
    }

    [Theory]
    [MemberData(nameof(CityCommandValidatorTestData.ValidTitles), MemberType = typeof(CityCommandValidatorTestData))]
    public void Validate_TitleIsValid_ShouldNotHaveValidationError(string title)
    {
        var command = Build(title, ValidCoordinates(), DefaultDescription);

        var result = Validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(TitleProperty);
    }

    [Fact]
    public void Validate_CoordinatesAreNull_ShouldHaveValidationError()
    {
        var command = Build(DefaultTitle, null, DefaultDescription);

        var result = Validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(CoordinatesProperty)
            .WithErrorMessage(ValidationConstants.City.CoordinatesRequired);
    }

    [Fact]
    public void Validate_CoordinatesAreInvalid_ShouldHaveNestedValidationError()
    {
        var command = Build(DefaultTitle, new CoordinatesDto(InvalidLatitude, 21.01), DefaultDescription);

        var result = Validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(LatitudeProperty)
            .WithErrorMessage(ValidationConstants.Coordinates.InvalidLatitude);
    }

    [Theory]
    [MemberData(nameof(CityCommandValidatorTestData.TooLongDescriptions), MemberType = typeof(CityCommandValidatorTestData))]
    public void Validate_DescriptionIsTooLong_ShouldHaveValidationError(string description)
    {
        var command = Build(DefaultTitle, ValidCoordinates(), description);

        var result = Validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(DescriptionProperty)
            .WithErrorMessage(ValidationConstants.City.DescriptionTooLong);
    }

    [Theory]
    [MemberData(nameof(CityCommandValidatorTestData.ValidDescriptions), MemberType = typeof(CityCommandValidatorTestData))]
    public void Validate_DescriptionIsValid_ShouldNotHaveValidationError(string? description)
    {
        var command = Build(DefaultTitle, ValidCoordinates(), description);

        var result = Validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(DescriptionProperty);
    }

    [Fact]
    public void Validate_CommandIsValid_ShouldNotHaveAnyValidationErrors()
    {
        var result = Validator.TestValidate(BuildValid());

        result.ShouldNotHaveAnyValidationErrors();
    }
}
