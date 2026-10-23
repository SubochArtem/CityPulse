using FluentValidation;

namespace Feedback.API.Features.Feedback.GetFilteredFeedback;

public sealed class GetFilteredFeedbackQueryValidator : AbstractValidator<GetFilteredFeedbackQuery>
{
    private const int MaxPageSize = 100;
    private const int MaxSearchTermLength = 100;
    private const string PageInvalid = "Page must be greater than or equal to 1.";
    private const string PageSizeInvalid = "Page size must be between 1 and 100.";
    private const string SearchTermTooLong = "Search term must not exceed 100 characters.";
    private const string DateRangeInvalid = "CreatedFrom must not be later than CreatedTo.";

    public GetFilteredFeedbackQueryValidator()
    {
        RuleFor(x => x.Filter.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage(PageInvalid);

        RuleFor(x => x.Filter.PageSize)
            .InclusiveBetween(1, MaxPageSize)
            .WithMessage(PageSizeInvalid);

        RuleFor(x => x.Filter.SearchTerm)
            .MaximumLength(MaxSearchTermLength)
            .WithMessage(SearchTermTooLong);

        RuleFor(x => x.Filter)
            .Must(f => f.CreatedFrom is null || f.CreatedTo is null || f.CreatedFrom <= f.CreatedTo)
            .WithMessage(DateRangeInvalid);
    }
}
