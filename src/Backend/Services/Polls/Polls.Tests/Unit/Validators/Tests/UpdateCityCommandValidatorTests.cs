using FluentValidation;
using FluentValidation.TestHelper;
using Polls.Application.Cities.Commands.UpdateCity;
using Polls.Application.Cities.DTOs;
using Polls.Application.Common.Constants;
using Xunit;

namespace Polls.Tests.Unit.Validators.Tests;

public sealed class UpdateCityCommandValidatorTests : CityCommandValidatorTestsBase<UpdateCityCommand>
{
    private const string DefaultTitle = "Warsaw";

    protected override IValidator<UpdateCityCommand> Validator { get; } = new UpdateCityCommandValidator();

    protected override UpdateCityCommand Build(string title, CoordinatesDto? coordinates, string? description) =>
        new(Guid.NewGuid(), title, coordinates!, description);

    [Fact]
    public void Validate_IdIsEmpty_ShouldHaveValidationError()
    {
        var command = new UpdateCityCommand(Guid.Empty, DefaultTitle, ValidCoordinates(), null);

        var result = Validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage(ValidationConstants.City.IdRequired);
    }

    [Fact]
    public void Validate_IdIsProvided_ShouldNotHaveValidationError()
    {
        var command = new UpdateCityCommand(Guid.NewGuid(), DefaultTitle, ValidCoordinates(), null);

        var result = Validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }
}
