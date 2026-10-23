using FluentValidation;
using Polls.Application.Common.Constants;

namespace Polls.Application.Ideas.Queries.GetIdeaCity;

public sealed class GetIdeaCityQueryValidator : AbstractValidator<GetIdeaCityQuery>
{
    public GetIdeaCityQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(ValidationConstants.Idea.IdRequired);
    }
}
