using FluentValidation;

namespace Feedback.API.Features.Activity.GetActivity;

public sealed class GetActivityQueryValidator : AbstractValidator<GetActivityQuery>
{
    private const int MaxPageSize = 50;
    private const string PageSizeInvalid = "Page size must be between 1 and 50.";
    private const string CityIdInvalid = "City id must not be empty.";
    private const string CursorInvalid = "Cursor is invalid.";

    public GetActivityQueryValidator()
    {
        RuleFor(x => x.Filter.PageSize)
            .InclusiveBetween(1, MaxPageSize)
            .WithMessage(PageSizeInvalid);

        RuleFor(x => x.Filter.CityId)
            .NotEqual(Guid.Empty)
            .When(x => x.Filter.CityId.HasValue)
            .WithMessage(CityIdInvalid);

        RuleFor(x => x.Filter.Cursor)
            .Must(cursor => ActivityCursor.TryDecode(cursor, out _))
            .When(x => x.Filter.Cursor is not null)
            .WithMessage(CursorInvalid);
    }
}
