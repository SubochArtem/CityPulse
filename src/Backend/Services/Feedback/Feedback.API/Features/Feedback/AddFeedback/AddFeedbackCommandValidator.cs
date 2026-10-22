using FluentValidation;

namespace Feedback.API.Features.Feedback.AddFeedback;

public sealed class AddFeedbackCommandValidator : AbstractValidator<AddFeedbackCommand>
{
    private const int MaxTextLength = 1000;
    private const string IdeaIdRequired = "Idea id is required.";
    private const string UserIdRequired = "User id is required.";
    private const string TextEmpty = "Comment text must not be empty.";
    private const string TextTooLong = "Comment text must not exceed 1000 characters.";

    public AddFeedbackCommandValidator()
    {
        RuleFor(x => x.IdeaId)
            .NotEmpty()
            .WithMessage(IdeaIdRequired);

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage(UserIdRequired);

        When(x => x.Text is not null, () =>
        {
            RuleFor(x => x.Text)
                .NotEmpty()
                .WithMessage(TextEmpty)
                .MaximumLength(MaxTextLength)
                .WithMessage(TextTooLong);
        });
    }
}
