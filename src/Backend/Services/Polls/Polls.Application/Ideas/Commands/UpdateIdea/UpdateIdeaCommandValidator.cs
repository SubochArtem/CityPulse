using FluentValidation;
using Polls.Application.Common.Constants;

namespace Polls.Application.Ideas.Commands.UpdateIdea;

public sealed class UpdateIdeaCommandValidator : AbstractValidator<UpdateIdeaCommand>
{
    public UpdateIdeaCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(ValidationConstants.Idea.IdRequired);

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
