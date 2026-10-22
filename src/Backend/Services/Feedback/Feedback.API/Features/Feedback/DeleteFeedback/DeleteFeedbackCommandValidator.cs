using FluentValidation;

namespace Feedback.API.Features.Feedback.DeleteFeedback;

public sealed class DeleteFeedbackCommandValidator : AbstractValidator<DeleteFeedbackCommand>
{
    private const string IdRequired = "Feedback id is required.";
    private const string UserIdRequired = "User id is required.";

    public DeleteFeedbackCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(IdRequired);

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage(UserIdRequired);
    }
}
