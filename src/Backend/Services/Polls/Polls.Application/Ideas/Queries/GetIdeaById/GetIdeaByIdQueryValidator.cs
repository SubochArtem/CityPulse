using FluentValidation;
using Polls.Application.Common.Constants;

namespace Polls.Application.Ideas.Queries.GetIdeaById;

public sealed class GetIdeaByIdQueryValidator : AbstractValidator<GetIdeaByIdQuery>
{
    public GetIdeaByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(ValidationConstants.Idea.IdRequired);
    }
}
