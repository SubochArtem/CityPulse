using FluentValidation;
using Polls.Application.Cities.Commands.CreateCity;
using Polls.Application.Cities.DTOs;

namespace Polls.Tests.Unit.Validators.Tests;

public sealed class CreateCityCommandValidatorTests : CityCommandValidatorTestsBase<CreateCityCommand>
{
    protected override IValidator<CreateCityCommand> Validator { get; } = new CreateCityCommandValidator();

    protected override CreateCityCommand Build(string title, CoordinatesDto? coordinates, string? description) =>
        new(title, coordinates!, description);
}
