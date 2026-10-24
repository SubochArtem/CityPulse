using FluentValidation.TestHelper;
using Polls.Application.Cities.Queries.GetCities;
using Polls.Application.Common.Constants;
using Polls.Application.Common.Models;
using Polls.Domain.Cities.Enums;
using Polls.Tests.Unit.Validators.TestData;
using Xunit;

namespace Polls.Tests.Unit.Validators.Tests;

public sealed class CityFilterValidatorTests
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 10;

    private readonly CityFilterValidator _validator = new();

    [Theory]
    [MemberData(nameof(CityFilterValidatorTestData.InvalidPages), MemberType = typeof(CityFilterValidatorTestData))]
    public void Validate_PageIsBelowMinimum_ShouldHaveValidationError(int page)
    {
        var filter = new CityFilter { Page = page, PageSize = DefaultPageSize };

        var result = _validator.TestValidate(filter);

        result.ShouldHaveValidationErrorFor(x => x.Page)
            .WithErrorMessage(ValidationConstants.Pagination.PageInvalid);
    }

    [Theory]
    [MemberData(nameof(CityFilterValidatorTestData.ValidPages), MemberType = typeof(CityFilterValidatorTestData))]
    public void Validate_PageIsValid_ShouldNotHaveValidationError(int page)
    {
        var filter = new CityFilter { Page = page, PageSize = DefaultPageSize };

        var result = _validator.TestValidate(filter);

        result.ShouldNotHaveValidationErrorFor(x => x.Page);
    }

    [Theory]
    [MemberData(nameof(CityFilterValidatorTestData.TooSmallPageSizes), MemberType = typeof(CityFilterValidatorTestData))]
    public void Validate_PageSizeIsBelowMinimum_ShouldHaveValidationError(int pageSize)
    {
        var filter = new CityFilter { Page = DefaultPage, PageSize = pageSize };

        var result = _validator.TestValidate(filter);

        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage(ValidationConstants.Pagination.PageSizeTooSmall);
    }

    [Theory]
    [MemberData(nameof(CityFilterValidatorTestData.TooLargePageSizes), MemberType = typeof(CityFilterValidatorTestData))]
    public void Validate_PageSizeIsAboveMaximum_ShouldHaveValidationError(int pageSize)
    {
        var filter = new CityFilter { Page = DefaultPage, PageSize = pageSize };

        var result = _validator.TestValidate(filter);

        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage(ValidationConstants.Pagination.PageSizeTooLarge);
    }

    [Theory]
    [MemberData(nameof(CityFilterValidatorTestData.ValidPageSizes), MemberType = typeof(CityFilterValidatorTestData))]
    public void Validate_PageSizeIsValid_ShouldNotHaveValidationError(int pageSize)
    {
        var filter = new CityFilter { Page = DefaultPage, PageSize = pageSize };

        var result = _validator.TestValidate(filter);

        result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
    }

    [Theory]
    [MemberData(nameof(CityFilterValidatorTestData.TooLongSearchTerms), MemberType = typeof(CityFilterValidatorTestData))]
    public void Validate_SearchTermIsTooLong_ShouldHaveValidationError(string searchTerm)
    {
        var filter = new CityFilter { SearchTerm = searchTerm };

        var result = _validator.TestValidate(filter);

        result.ShouldHaveValidationErrorFor(x => x.SearchTerm)
            .WithErrorMessage(ValidationConstants.Search.SearchTermTooLong);
    }

    [Theory]
    [MemberData(nameof(CityFilterValidatorTestData.ValidSearchTerms), MemberType = typeof(CityFilterValidatorTestData))]
    public void Validate_SearchTermIsValidOrBlank_ShouldNotHaveValidationError(string? searchTerm)
    {
        var filter = new CityFilter { SearchTerm = searchTerm };

        var result = _validator.TestValidate(filter);

        result.ShouldNotHaveValidationErrorFor(x => x.SearchTerm);
    }

    [Theory]
    [MemberData(nameof(CityFilterValidatorTestData.UnknownStatuses), MemberType = typeof(CityFilterValidatorTestData))]
    public void Validate_StatusIsNotDefinedInEnum_ShouldHaveValidationError(CityStatus status)
    {
        var filter = new CityFilter { Status = status };

        var result = _validator.TestValidate(filter);

        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage(ValidationConstants.City.InvalidStatus);
    }

    [Theory]
    [MemberData(nameof(CityFilterValidatorTestData.ValidStatuses), MemberType = typeof(CityFilterValidatorTestData))]
    public void Validate_StatusIsNullOrDefinedInEnum_ShouldNotHaveValidationError(CityStatus? status)
    {
        var filter = new CityFilter { Status = status };

        var result = _validator.TestValidate(filter);

        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Validate_DefaultFilter_ShouldNotHaveAnyValidationErrors()
    {
        var result = _validator.TestValidate(new CityFilter());

        result.ShouldNotHaveAnyValidationErrors();
    }
}
