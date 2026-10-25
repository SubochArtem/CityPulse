using Feedback.API.Features.Feedback.GetFilteredFeedback;
using Feedback.Tests.Unit.Validators.TestData;
using FluentValidation.TestHelper;
using Xunit;

namespace Feedback.Tests.Unit.Validators.Tests;

public sealed class GetFilteredFeedbackQueryValidatorTests
{
    private const string PageInvalidMessage = "Page must be greater than or equal to 1.";
    private const string PageSizeInvalidMessage = "Page size must be between 1 and 100.";
    private const string SearchTermTooLongMessage = "Search term must not exceed 100 characters.";
    private const string DateRangeInvalidMessage = "CreatedFrom must not be later than CreatedTo.";

    private readonly GetFilteredFeedbackQueryValidator _validator = new();

    private static GetFilteredFeedbackQuery BuildQuery(FeedbackFilter filter) => new(filter);

    [Theory]
    [MemberData(nameof(GetFilteredFeedbackQueryValidatorTestData.InvalidPages), MemberType = typeof(GetFilteredFeedbackQueryValidatorTestData))]
    public void Validate_PageIsBelowMinimum_ShouldHaveValidationError(int page)
    {
        var result = _validator.TestValidate(BuildQuery(new FeedbackFilter { Page = page }));

        result.ShouldHaveValidationErrorFor(x => x.Filter.Page)
            .WithErrorMessage(PageInvalidMessage);
    }

    [Theory]
    [MemberData(nameof(GetFilteredFeedbackQueryValidatorTestData.ValidPages), MemberType = typeof(GetFilteredFeedbackQueryValidatorTestData))]
    public void Validate_PageIsValid_ShouldNotHaveValidationError(int page)
    {
        var result = _validator.TestValidate(BuildQuery(new FeedbackFilter { Page = page }));

        result.ShouldNotHaveValidationErrorFor(x => x.Filter.Page);
    }

    [Theory]
    [MemberData(nameof(GetFilteredFeedbackQueryValidatorTestData.InvalidPageSizes), MemberType = typeof(GetFilteredFeedbackQueryValidatorTestData))]
    public void Validate_PageSizeIsOutOfRange_ShouldHaveValidationError(int pageSize)
    {
        var result = _validator.TestValidate(BuildQuery(new FeedbackFilter { PageSize = pageSize }));

        result.ShouldHaveValidationErrorFor(x => x.Filter.PageSize)
            .WithErrorMessage(PageSizeInvalidMessage);
    }

    [Theory]
    [MemberData(nameof(GetFilteredFeedbackQueryValidatorTestData.ValidPageSizes), MemberType = typeof(GetFilteredFeedbackQueryValidatorTestData))]
    public void Validate_PageSizeIsValid_ShouldNotHaveValidationError(int pageSize)
    {
        var result = _validator.TestValidate(BuildQuery(new FeedbackFilter { PageSize = pageSize }));

        result.ShouldNotHaveValidationErrorFor(x => x.Filter.PageSize);
    }

    [Theory]
    [MemberData(nameof(GetFilteredFeedbackQueryValidatorTestData.TooLongSearchTerms), MemberType = typeof(GetFilteredFeedbackQueryValidatorTestData))]
    public void Validate_SearchTermIsTooLong_ShouldHaveValidationError(string searchTerm)
    {
        var result = _validator.TestValidate(BuildQuery(new FeedbackFilter { SearchTerm = searchTerm }));

        result.ShouldHaveValidationErrorFor(x => x.Filter.SearchTerm)
            .WithErrorMessage(SearchTermTooLongMessage);
    }

    [Theory]
    [MemberData(nameof(GetFilteredFeedbackQueryValidatorTestData.ValidSearchTerms), MemberType = typeof(GetFilteredFeedbackQueryValidatorTestData))]
    public void Validate_SearchTermIsValid_ShouldNotHaveValidationError(string? searchTerm)
    {
        var result = _validator.TestValidate(BuildQuery(new FeedbackFilter { SearchTerm = searchTerm }));

        result.ShouldNotHaveValidationErrorFor(x => x.Filter.SearchTerm);
    }

    [Theory]
    [MemberData(nameof(GetFilteredFeedbackQueryValidatorTestData.InvalidDateRanges), MemberType = typeof(GetFilteredFeedbackQueryValidatorTestData))]
    public void Validate_CreatedFromIsLaterThanCreatedTo_ShouldHaveValidationError(
        DateTimeOffset? createdFrom,
        DateTimeOffset? createdTo)
    {
        var filter = new FeedbackFilter { CreatedFrom = createdFrom, CreatedTo = createdTo };

        var result = _validator.TestValidate(BuildQuery(filter));

        result.ShouldHaveValidationErrorFor(x => x.Filter)
            .WithErrorMessage(DateRangeInvalidMessage);
    }

    [Theory]
    [MemberData(nameof(GetFilteredFeedbackQueryValidatorTestData.ValidDateRanges), MemberType = typeof(GetFilteredFeedbackQueryValidatorTestData))]
    public void Validate_DateRangeIsValid_ShouldNotHaveValidationError(
        DateTimeOffset? createdFrom,
        DateTimeOffset? createdTo)
    {
        var filter = new FeedbackFilter { CreatedFrom = createdFrom, CreatedTo = createdTo };

        var result = _validator.TestValidate(BuildQuery(filter));

        result.ShouldNotHaveValidationErrorFor(x => x.Filter);
    }

    [Fact]
    public void Validate_FilterIsDefault_ShouldNotHaveAnyValidationErrors()
    {
        var result = _validator.TestValidate(BuildQuery(new FeedbackFilter()));

        result.ShouldNotHaveAnyValidationErrors();
    }
}
