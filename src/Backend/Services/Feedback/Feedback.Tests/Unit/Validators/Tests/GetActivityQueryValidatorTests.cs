using Feedback.API.Features.Activity.GetActivity;
using FluentValidation.TestHelper;
using Xunit;

namespace Feedback.Tests.Unit.Validators.Tests;

public sealed class GetActivityQueryValidatorTests
{
    private const string PageSizeInvalidMessage = "Page size must be between 1 and 50.";
    private const string CityIdInvalidMessage = "City id must not be empty.";
    private const string CursorInvalidMessage = "Cursor is invalid.";

    private readonly GetActivityQueryValidator _validator = new();

    private static GetActivityQuery BuildQuery(ActivityFilter filter) => new(filter);

    [Fact]
    public void Validate_DefaultFilter_ShouldNotHaveErrors()
    {
        var result = _validator.TestValidate(BuildQuery(new ActivityFilter()));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(51)]
    public void Validate_PageSizeIsOutOfRange_ShouldHaveValidationError(int pageSize)
    {
        var result = _validator.TestValidate(BuildQuery(new ActivityFilter { PageSize = pageSize }));

        result.ShouldHaveValidationErrorFor(x => x.Filter.PageSize)
            .WithErrorMessage(PageSizeInvalidMessage);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    public void Validate_PageSizeIsInRange_ShouldNotHaveValidationError(int pageSize)
    {
        var result = _validator.TestValidate(BuildQuery(new ActivityFilter { PageSize = pageSize }));

        result.ShouldNotHaveValidationErrorFor(x => x.Filter.PageSize);
    }

    [Fact]
    public void Validate_CityIdIsEmpty_ShouldHaveValidationError()
    {
        var result = _validator.TestValidate(BuildQuery(new ActivityFilter { CityId = Guid.Empty }));

        result.ShouldHaveValidationErrorFor(x => x.Filter.CityId)
            .WithErrorMessage(CityIdInvalidMessage);
    }

    [Fact]
    public void Validate_CursorIsMalformed_ShouldHaveValidationError()
    {
        var result = _validator.TestValidate(BuildQuery(new ActivityFilter { Cursor = "broken" }));

        result.ShouldHaveValidationErrorFor(x => x.Filter.Cursor)
            .WithErrorMessage(CursorInvalidMessage);
    }

    [Fact]
    public void Validate_CursorIsValid_ShouldNotHaveValidationError()
    {
        var cursor = new ActivityCursor(DateTimeOffset.UtcNow, Guid.NewGuid()).Encode();

        var result = _validator.TestValidate(BuildQuery(new ActivityFilter { Cursor = cursor }));

        result.ShouldNotHaveValidationErrorFor(x => x.Filter.Cursor);
    }
}
