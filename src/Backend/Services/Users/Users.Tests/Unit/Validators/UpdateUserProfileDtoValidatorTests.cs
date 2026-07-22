using FluentValidation.TestHelper;
using Users.Business.Constants;
using Users.Business.DTOs;
using Users.Business.Validators;
using Xunit;

namespace Users.Tests.Unit.Validators;

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
    [MemberData(nameof(EmptyNicknames))]
    public void Validate_NicknameIsEmpty_ShouldHaveValidationError(string nickname)
    {
        var dto = new UpdateUserProfileDto { Nickname = nickname, CityId = null };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Nickname)
            .WithErrorMessage(ValidationConstants.User.NicknameRequired);
    }

    [Theory]
    [MemberData(nameof(TooShortNicknames))]
    public void Validate_NicknameIsTooShort_ShouldHaveValidationError(string nickname)
    {
        var dto = new UpdateUserProfileDto { Nickname = nickname, CityId = null };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Nickname)
            .WithErrorMessage(ValidationConstants.User.NicknameTooShort);
    }

    [Fact]
    public void Validate_NicknameIsTooLong_ShouldHaveValidationError()
    {
        var nickname = new string('a', ValidationConstants.User.MaxNicknameLength + 1);
        var dto = new UpdateUserProfileDto { Nickname = nickname, CityId = null };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Nickname)
            .WithErrorMessage(ValidationConstants.User.NicknameTooLong);
    }

    [Theory]
    [MemberData(nameof(InvalidCharacterNicknames))]
    public void Validate_NicknameHasInvalidCharacters_ShouldHaveValidationError(string nickname)
    {
        var dto = new UpdateUserProfileDto { Nickname = nickname, CityId = null };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Nickname)
            .WithErrorMessage(ValidationConstants.User.NicknameInvalidCharacters);
    }

    [Theory]
    [MemberData(nameof(ConsecutiveSpecialCharacterNicknames))]
    public void Validate_NicknameHasConsecutiveSpecialCharacters_ShouldHaveValidationError(string nickname)
    {
        var dto = new UpdateUserProfileDto { Nickname = nickname, CityId = null };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Nickname)
            .WithErrorMessage(ValidationConstants.User.NicknameConsecutiveSpecialCharacters);
    }

    [Theory]
    [MemberData(nameof(ValidNicknames))]
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
            .WithErrorMessage(ValidationConstants.User.NicknameTooShort);
        result.ShouldNotHaveValidationErrorFor(x => x.CityId);
    }
    
    public static TheoryData<string> EmptyNicknames => new()
    {
        "",
        "   "
    };

    public static TheoryData<string> TooShortNicknames => new()
    {
        "a",
        "aa"
    };

    public static TheoryData<string> InvalidCharacterNicknames => new()
    {
        "abc def",
        "abc!def",
        "abc#def",
        "abc@def",
        "abc/def"
    };

    public static TheoryData<string> ConsecutiveSpecialCharacterNicknames => new()
    {
        "abc..def",
        "abc__def",
        "abc++def",
        "abc--def",
        "abc.-def",
        "abc_+def"
    };

    public static TheoryData<string> ValidNicknames => new()
    {
        "abc",
        "abc_def",
        "abc.def",
        "abc-def",
        "abc+def",
        "abcdef123",
        new string('a', ValidationConstants.User.MaxNicknameLength)
    };
}
