using FluentValidation;

namespace Feedback.API.Features.Feedback.EditComment;

public sealed class EditCommentCommandValidator : AbstractValidator<EditCommentCommand>
{
    private const int MaxTextLength = 1000;
    private const string IdRequired = "Feedback id is required.";
    private const string UserIdRequired = "User id is required.";
    private const string TextEmpty = "Comment text must not be empty.";
    private const string TextTooLong = "Comment text must not exceed 1000 characters.";

    public EditCommentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(IdRequired);

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage(UserIdRequired);

        RuleFor(x => x.Text)
            .NotEmpty()
            .WithMessage(TextEmpty)
            .MaximumLength(MaxTextLength)
            .WithMessage(TextTooLong);
    }
}
