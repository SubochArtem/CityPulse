using FluentValidation.TestHelper;
using Polls.Application.Cities.Commands.ChangeStatus;
using Polls.Application.Common.Constants;
using Polls.Domain.Cities.Enums;
using Polls.Tests.Unit.Validators.TestData;
using Xunit;

namespace Polls.Tests.Unit.Validators.Tests;

public sealed class ChangeCityStatusCommandValidatorTests
{
    private readonly ChangeCityStatusCommandValidator _validator = new();

    [Fact]
    public void Validate_IdIsEmpty_ShouldHaveValidationError()
    {
        var command = new ChangeCityStatusCommand(Guid.Empty, CityStatus.Active);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage(ValidationConstants.City.IdRequired);
    }

    [Fact]
    public void Validate_IdIsProvided_ShouldNotHaveValidationError()
    {
        var command = new ChangeCityStatusCommand(Guid.NewGuid(), CityStatus.Active);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }

    [Theory]
    [MemberData(nameof(ChangeCityStatusCommandValidatorTestData.UnknownStatuses), MemberType = typeof(ChangeCityStatusCommandValidatorTestData))]
    public void Validate_StatusIsNotDefinedInEnum_ShouldHaveValidationError(CityStatus status)
    {
        var command = new ChangeCityStatusCommand(Guid.NewGuid(), status);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.NewStatus)
            .WithErrorMessage(ValidationConstants.City.InvalidStatus);
    }

    [Theory]
    [MemberData(nameof(ChangeCityStatusCommandValidatorTestData.DefinedStatuses), MemberType = typeof(ChangeCityStatusCommandValidatorTestData))]
    public void Validate_StatusIsDefinedInEnum_ShouldNotHaveValidationError(CityStatus status)
    {
        var command = new ChangeCityStatusCommand(Guid.NewGuid(), status);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.NewStatus);
    }

    [Fact]
    public void Validate_CommandIsValid_ShouldNotHaveAnyValidationErrors()
    {
        var command = new ChangeCityStatusCommand(Guid.NewGuid(), CityStatus.Inactive);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
