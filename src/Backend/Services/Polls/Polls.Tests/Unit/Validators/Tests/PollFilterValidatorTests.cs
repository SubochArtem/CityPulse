using FluentValidation.TestHelper;
using Polls.Application.Common.Constants;
using Polls.Application.Common.Models;
using Polls.Application.Polls.Queries.GetPolls;
using Polls.Domain.Polls.Enums;
using Polls.Tests.Unit.Validators.TestData;
using Xunit;

namespace Polls.Tests.Unit.Validators.Tests;

public sealed class PollFilterValidatorTests
{
    private readonly PollFilterValidator _validator = new();

    [Fact]
    public void Validate_CityIdIsEmpty_ShouldHaveValidationError()
    {
        var filter = new PollFilter { CityId = Guid.Empty };

        var result = _validator.TestValidate(filter);

        result.ShouldHaveValidationErrorFor(x => x.CityId)
            .WithErrorMessage(ValidationConstants.City.IdRequired);
    }

    [Fact]
    public void Validate_CityIdIsProvided_ShouldNotHaveValidationError()
    {
        var filter = new PollFilter { CityId = Guid.NewGuid() };

        var result = _validator.TestValidate(filter);

        result.ShouldNotHaveValidationErrorFor(x => x.CityId);
    }

    [Fact]
    public void Validate_CityIdIsNull_ShouldNotHaveValidationError()
    {
        var result = _validator.TestValidate(new PollFilter { CityId = null });

        result.ShouldNotHaveValidationErrorFor(x => x.CityId);
    }

    [Theory]
    [MemberData(nameof(PollFilterValidatorTestData.UnknownTypes), MemberType = typeof(PollFilterValidatorTestData))]
    public void Validate_TypeIsNotDefinedInEnum_ShouldHaveValidationError(PollType type)
    {
        var filter = new PollFilter { Type = type };

        var result = _validator.TestValidate(filter);

        result.ShouldHaveValidationErrorFor(x => x.Type)
            .WithErrorMessage(ValidationConstants.Poll.InvalidType);
    }

    [Theory]
    [MemberData(nameof(PollFilterValidatorTestData.ValidTypes), MemberType = typeof(PollFilterValidatorTestData))]
    public void Validate_TypeIsNullOrDefinedInEnum_ShouldNotHaveValidationError(PollType? type)
    {
        var filter = new PollFilter { Type = type };

        var result = _validator.TestValidate(filter);

        result.ShouldNotHaveValidationErrorFor(x => x.Type);
    }

    [Theory]
    [MemberData(nameof(PollFilterValidatorTestData.UnknownStatuses), MemberType = typeof(PollFilterValidatorTestData))]
    public void Validate_StatusIsNotDefinedInEnum_ShouldHaveValidationError(PollStatus status)
    {
        var filter = new PollFilter { Status = status };

        var result = _validator.TestValidate(filter);

        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage(ValidationConstants.Poll.InvalidStatus);
    }

    [Theory]
    [MemberData(nameof(PollFilterValidatorTestData.ValidStatuses), MemberType = typeof(PollFilterValidatorTestData))]
    public void Validate_StatusIsNullOrDefinedInEnum_ShouldNotHaveValidationError(PollStatus? status)
    {
        var filter = new PollFilter { Status = status };

        var result = _validator.TestValidate(filter);

        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    [Theory]
    [MemberData(nameof(PollFilterValidatorTestData.EndsBeforeValues), MemberType = typeof(PollFilterValidatorTestData))]
    public void Validate_EndsBeforeHasAnyValue_ShouldNotHaveValidationError(DateTimeOffset? endsBefore)
    {
        var filter = new PollFilter { EndsBefore = endsBefore };

        var result = _validator.TestValidate(filter);

        result.ShouldNotHaveValidationErrorFor(x => x.EndsBefore);
    }

    [Fact]
    public void Validate_PageIsBelowMinimum_ShouldHaveValidationErrorFromBaseFilterRules()
    {
        var filter = new PollFilter { Page = ValidationConstants.Pagination.MinPage - 1 };

        var result = _validator.TestValidate(filter);

        result.ShouldHaveValidationErrorFor(x => x.Page)
            .WithErrorMessage(ValidationConstants.Pagination.PageInvalid);
    }

    [Fact]
    public void Validate_PageSizeIsAboveMaximum_ShouldHaveValidationErrorFromBaseFilterRules()
    {
        var filter = new PollFilter { PageSize = ValidationConstants.Pagination.MaxPageSize + 1 };

        var result = _validator.TestValidate(filter);

        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage(ValidationConstants.Pagination.PageSizeTooLarge);
    }

    [Fact]
    public void Validate_DefaultFilter_ShouldNotHaveAnyValidationErrors()
    {
        var result = _validator.TestValidate(new PollFilter());

        result.ShouldNotHaveAnyValidationErrors();
    }
}
