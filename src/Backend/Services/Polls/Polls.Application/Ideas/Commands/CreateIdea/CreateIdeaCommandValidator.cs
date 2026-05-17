using FluentValidation;
using Polls.Application.Common.Constants;

namespace Polls.Application.Ideas.Commands.CreateIdea;

public sealed class CreateIdeaCommandValidator : AbstractValidator<CreateIdeaCommand>
{
    public CreateIdeaCommandValidator()
    {
        RuleFor(x => x.PollId)
            .NotEmpty()
            .WithMessage(ValidationConstants.Poll.IdRequired);

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage(ValidationConstants.Idea.UserIdRequired);

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage(ValidationConstants.Idea.TitleRequired)
            .MaximumLength(ValidationConstants.Idea.MaxTitleLength)
            .WithMessage(ValidationConstants.Idea.TitleTooLong);

        RuleFor(x => x.Description)
            .MaximumLength(ValidationConstants.Idea.MaxDescriptionLength)
            .WithMessage(ValidationConstants.Idea.DescriptionTooLong);
    }
}
