using FluentValidation.TestHelper;
using Users.Business.Constants;
using Users.Business.DTOs;
using Users.Business.Validators;
using Users.Tests.Unit.Validators.TestData;
using Xunit;

namespace Users.Tests.Unit.Validators.Tests;

public sealed class UpdateUserProfileDtoValidatorTests
{
    private const string DefaultValidNickname = "some_nickname";
    private const string TooShortNickname = "ab";

    private readonly UpdateUserProfileDtoValidator _validator = new();

    [Fact]
    public void Validate_NicknameAndCityIdAreNull_ShouldNotHaveAnyValidationErrors()
    {
        var dto = new UpdateUserProfileDto { Nickname = null, CityId = null };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [MemberData(nameof(UpdateUserProfileDtoValidatorTestData.EmptyNicknames), MemberType = typeof(UpdateUserProfileDtoValidatorTestData))]
    public void Validate_NicknameIsEmpty_ShouldHaveValidationError(string nickname)
    {
        var dto = new UpdateUserProfileDto { Nickname = nickname, CityId = null };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Nickname)
            .WithErrorMessage(ValidationConstants.Nickname.Required);
    }

    [Theory]
    [MemberData(nameof(UpdateUserProfileDtoValidatorTestData.TooShortNicknames), MemberType = typeof(UpdateUserProfileDtoValidatorTestData))]
    public void Validate_NicknameIsTooShort_ShouldHaveValidationError(string nickname)
    {
        var dto = new UpdateUserProfileDto { Nickname = nickname, CityId = null };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Nickname)
            .WithErrorMessage(ValidationConstants.Nickname.TooShort);
    }

    [Fact]
    public void Validate_NicknameIsTooLong_ShouldHaveValidationError()
    {
        var nickname = new string('a', ValidationConstants.Nickname.MaxLength + 1);
        var dto = new UpdateUserProfileDto { Nickname = nickname, CityId = null };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Nickname)
            .WithErrorMessage(ValidationConstants.Nickname.TooLong);
    }

    [Theory]
    [MemberData(nameof(UpdateUserProfileDtoValidatorTestData.InvalidCharacterNicknames), MemberType = typeof(UpdateUserProfileDtoValidatorTestData))]
    public void Validate_NicknameHasInvalidCharacters_ShouldHaveValidationError(string nickname)
    {
        var dto = new UpdateUserProfileDto { Nickname = nickname, CityId = null };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Nickname)
            .WithErrorMessage(ValidationConstants.Nickname.InvalidCharacters);
    }

    [Theory]
    [MemberData(nameof(UpdateUserProfileDtoValidatorTestData.ConsecutiveSpecialCharacterNicknames), MemberType = typeof(UpdateUserProfileDtoValidatorTestData))]
    public void Validate_NicknameHasConsecutiveSpecialCharacters_ShouldHaveValidationError(string nickname)
    {
        var dto = new UpdateUserProfileDto { Nickname = nickname, CityId = null };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Nickname)
            .WithErrorMessage(ValidationConstants.Nickname.ConsecutiveSpecialCharacters);
    }

    [Theory]
    [MemberData(nameof(UpdateUserProfileDtoValidatorTestData.ValidNicknames), MemberType = typeof(UpdateUserProfileDtoValidatorTestData))]
    public void Validate_NicknameIsValid_ShouldNotHaveValidationError(string nickname)
    {
        var dto = new UpdateUserProfileDto { Nickname = nickname, CityId = null };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Nickname);
    }

    [Fact]
    public void Validate_CityIdIsProvided_ShouldNotHaveValidationError()
    {
        var dto = new UpdateUserProfileDto { Nickname = null, CityId = Guid.NewGuid() };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.CityId);
    }

    [Fact]
    public void Validate_AllPropertiesValid_ShouldNotHaveAnyValidationErrors()
    {
        var dto = new UpdateUserProfileDto { Nickname = DefaultValidNickname, CityId = Guid.NewGuid() };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_NicknameIsInvalidAndCityIdIsValid_ShouldOnlyHaveNicknameError()
    {
        var dto = new UpdateUserProfileDto { Nickname = TooShortNickname, CityId = Guid.NewGuid() };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Nickname)
            .WithErrorMessage(ValidationConstants.Nickname.TooShort);
        result.ShouldNotHaveValidationErrorFor(x => x.CityId);
    }
}
