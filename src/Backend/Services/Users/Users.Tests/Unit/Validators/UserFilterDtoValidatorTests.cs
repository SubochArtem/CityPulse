using FluentValidation.TestHelper;
using Users.Business.Constants;
using Users.Business.DTOs;
using Users.Business.Validators;
using Xunit;

namespace Users.Tests.Unit.Validators;

public sealed class UserFilterDtoValidatorTests
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 10;
    private const string DefaultValidNickname = "some_nickname";

    private readonly UserFilterDtoValidator _validator = new();

    [Theory]
    [MemberData(nameof(InvalidPages))]
    public void Validate_PageIsBelowMinimum_ShouldHaveValidationError(int page)
    {
        var dto = new UserFilterDto { Page = page, PageSize = DefaultPageSize };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Page)
            .WithErrorMessage(ValidationConstants.Pagination.PageInvalid);
    }

    [Theory]
    [MemberData(nameof(ValidPages))]
    public void Validate_PageIsValid_ShouldNotHaveValidationError(int page)
    {
        var dto = new UserFilterDto { Page = page, PageSize = DefaultPageSize };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Page);
    }

    [Theory]
    [MemberData(nameof(TooSmallPageSizes))]
    public void Validate_PageSizeIsBelowMinimum_ShouldHaveValidationError(int pageSize)
    {
        var dto = new UserFilterDto { Page = DefaultPage, PageSize = pageSize };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage(ValidationConstants.Pagination.PageSizeTooSmall);
    }

    [Theory]
    [MemberData(nameof(TooLargePageSizes))]
    public void Validate_PageSizeIsAboveMaximum_ShouldHaveValidationError(int pageSize)
    {
        var dto = new UserFilterDto { Page = DefaultPage, PageSize = pageSize };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage(ValidationConstants.Pagination.PageSizeTooLarge);
    }

    [Theory]
    [MemberData(nameof(ValidPageSizes))]
    public void Validate_PageSizeIsWithinBounds_ShouldNotHaveValidationError(int pageSize)
    {
        var dto = new UserFilterDto { Page = DefaultPage, PageSize = pageSize };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Validate_NicknameAndCityIdAreNull_ShouldNotHaveValidationErrors()
    {
        var dto = new UserFilterDto { Page = DefaultPage, PageSize = DefaultPageSize, Nickname = null, CityId = null };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Nickname);
        result.ShouldNotHaveValidationErrorFor(x => x.CityId);
    }

    [Fact]
    public void Validate_AllPropertiesValid_ShouldNotHaveAnyValidationErrors()
    {
        const int validPage = 2;
        const int validPageSize = 25;

        var dto = new UserFilterDto
        {
            Page = validPage,
            PageSize = validPageSize,
            Nickname = DefaultValidNickname,
            CityId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_PageAndPageSizeAreInvalid_ShouldHaveValidationErrorsForBothProperties()
    {
        var dto = new UserFilterDto
        {
            Page = 0,
            PageSize = ValidationConstants.Pagination.MaxPageSize + 1,
            Nickname = DefaultValidNickname,
            CityId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Page)
            .WithErrorMessage(ValidationConstants.Pagination.PageInvalid);
        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorMessage(ValidationConstants.Pagination.PageSizeTooLarge);
    }
    
    public static TheoryData<int> InvalidPages => new()
    {
        0,
        -1,
        int.MinValue
    };

    public static TheoryData<int> ValidPages => new()
    {
        ValidationConstants.Pagination.MinPage,
        ValidationConstants.Pagination.MinPage + 1,
        1000
    };

    public static TheoryData<int> TooSmallPageSizes => new()
    {
        0,
        -1
    };

    public static TheoryData<int> TooLargePageSizes => new()
    {
        ValidationConstants.Pagination.MaxPageSize + 1,
        int.MaxValue
    };

    public static TheoryData<int> ValidPageSizes => new()
    {
        ValidationConstants.Pagination.MinPageSize,
        ValidationConstants.Pagination.MaxPageSize,
        50
    };
}
