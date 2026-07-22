using FluentValidation.TestHelper;
using Users.Business.Constants;
using Users.Business.DTOs;
using Users.Business.Validators;
using Xunit;

namespace Users.Tests.Unit.Validators;

public sealed class CreateUserValidatorTests
{
    private const string DefaultIdentityId = "auth0|abc123";
    private const string DefaultNickname = "some_nickname";

    private readonly CreateUserValidator _validator = new();

    [Theory]
    [MemberData(nameof(EmptyIdentityIds))]
    public void Validate_IdentityIdIsEmpty_ShouldHaveValidationError(string? identityId)
    {
        var dto = new CreateUserDto { IdentityId = identityId!, Nickname = DefaultNickname };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.IdentityId)
            .WithErrorMessage(ValidationConstants.User.IdentityIdRequired);
    }

    [Theory]
    [MemberData(nameof(InvalidIdentityIds))]
    public void Validate_IdentityIdFormatIsInvalid_ShouldHaveValidationError(string identityId)
    {
        var dto = new CreateUserDto { IdentityId = identityId, Nickname = DefaultNickname };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.IdentityId)
            .WithErrorMessage(ValidationConstants.User.IdentityIdInvalidFormat);
    }

    [Theory]
    [MemberData(nameof(ValidIdentityIds))]
    public void Validate_IdentityIdFormatIsValid_ShouldNotHaveValidationError(string identityId)
    {
        var dto = new CreateUserDto { IdentityId = identityId, Nickname = DefaultNickname };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.IdentityId);
    }

    [Theory]
    [MemberData(nameof(EmptyNicknames))]
    public void Validate_NicknameIsEmptyOrNull_ShouldNotHaveValidationError(string? nickname)
    {
        var dto = new CreateUserDto { IdentityId = DefaultIdentityId, Nickname = nickname! };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Nickname);
    }
    
    public static TheoryData<string?> EmptyIdentityIds => new()
    {
        null,
        "",
        "   "
    };

    public static TheoryData<string> InvalidIdentityIds => new()
    {
        "auth0",
        "auth0|",
        "|abc123",
        "auth0 abc123",
        "auth0|abc 123",
        "auth0|abc|123"
    };

    public static TheoryData<string> ValidIdentityIds => new()
    {
        "auth0|abc123",
        "google-oauth2|1234567890",
        "windowslive|abc.def-123_456"
    };

    public static TheoryData<string?> EmptyNicknames => new()
    {
        null,
        ""
    };
}
