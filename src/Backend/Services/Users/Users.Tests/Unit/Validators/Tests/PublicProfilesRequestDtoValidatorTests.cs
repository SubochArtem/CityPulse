using FluentValidation.TestHelper;
using Users.Business.Constants;
using Users.Business.DTOs;
using Users.Business.Validators;
using Xunit;

namespace Users.Tests.Unit.Validators.Tests;

public sealed class PublicProfilesRequestDtoValidatorTests
{
    private readonly PublicProfilesRequestDtoValidator _validator = new();

    [Fact]
    public void Validate_IdsAreEmpty_ShouldHaveValidationError()
    {
        var dto = new PublicProfilesRequestDto { Ids = [] };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Ids)
            .WithErrorMessage(ValidationConstants.PublicProfiles.IdsRequired);
    }

    [Fact]
    public void Validate_IdsAreAboveMaximum_ShouldHaveValidationError()
    {
        var dto = new PublicProfilesRequestDto { Ids = CreateIds(ValidationConstants.PublicProfiles.MaxIds + 1) };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Ids)
            .WithErrorMessage(ValidationConstants.PublicProfiles.TooManyIds);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(ValidationConstants.PublicProfiles.MaxIds)]
    public void Validate_IdsCountIsWithinBounds_ShouldNotHaveValidationError(int count)
    {
        var dto = new PublicProfilesRequestDto { Ids = CreateIds(count) };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    private static Guid[] CreateIds(int count) =>
        Enumerable.Range(0, count).Select(_ => Guid.NewGuid()).ToArray();
}
