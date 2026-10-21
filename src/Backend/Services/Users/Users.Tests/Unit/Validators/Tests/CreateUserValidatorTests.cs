using FluentValidation.TestHelper;
using Users.Business.Constants;
using Users.Business.DTOs;
using Users.Business.Validators;
using Users.Tests.Unit.Validators.TestData;
using Xunit;

namespace Users.Tests.Unit.Validators.Tests;

public sealed class CreateUserValidatorTests
{
    private const string DefaultIdentityId = "auth0|abc123";
    private const string DefaultNickname = "some_nickname";
    private readonly CreateUserValidator _validator = new();

    [Theory]
    [MemberData(nameof(CreateUserValidatorTestData.EmptyIdentityIds), MemberType = typeof(CreateUserValidatorTestData))]
    public void Validate_IdentityIdIsEmpty_ShouldHaveValidationError(string? identityId)
    {
        var dto = new CreateUserDto { IdentityId = identityId!, Nickname = DefaultNickname };
        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.IdentityId)
            .WithErrorMessage(ValidationConstants.IdentityId.Required);
    }

    [Theory]
    [MemberData(nameof(CreateUserValidatorTestData.InvalidIdentityIds), MemberType = typeof(CreateUserValidatorTestData))]
    public void Validate_IdentityIdFormatIsInvalid_ShouldHaveValidationError(string identityId)
    {
        var dto = new CreateUserDto { IdentityId = identityId, Nickname = DefaultNickname };
        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.IdentityId)
            .WithErrorMessage(ValidationConstants.IdentityId.InvalidFormat);
    }

    [Theory]
    [MemberData(nameof(CreateUserValidatorTestData.ValidIdentityIds), MemberType = typeof(CreateUserValidatorTestData))]
    public void Validate_IdentityIdFormatIsValid_ShouldNotHaveValidationError(string identityId)
    {
        var dto = new CreateUserDto { IdentityId = identityId, Nickname = DefaultNickname };
        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.IdentityId);
    }

    [Theory]
    [MemberData(nameof(CreateUserValidatorTestData.EmptyNicknames), MemberType = typeof(CreateUserValidatorTestData))]
    public void Validate_NicknameIsEmptyOrNull_ShouldNotHaveValidationError(string? nickname)
    {
        var dto = new CreateUserDto { IdentityId = DefaultIdentityId, Nickname = nickname! };
        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Nickname);
    }
}
