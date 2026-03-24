using FluentValidation;
using Polls.Application.Common.Constants;

namespace Polls.Application.Ideas.Queries.GetIdeas;

public sealed class GetIdeasQueryValidator : AbstractValidator<GetIdeasQuery>
{
    public GetIdeasQueryValidator()
    {
        RuleFor(x => x.Filter)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage(ValidationConstants.Pagination.FilterRequired);
        
        RuleFor(x => x.Filter.Page)
            .GreaterThanOrEqualTo(ValidationConstants.Pagination.MinPage)
            .WithMessage(ValidationConstants.Pagination.PageInvalid);
        
        RuleFor(x => x.Filter.PollId)
            .NotEmpty()
            .WithMessage(ValidationConstants.Poll.IdRequired);

        RuleFor(x => x.Filter.PageSize)
            .GreaterThanOrEqualTo(ValidationConstants.Pagination.MinPageSize)
            .WithMessage(ValidationConstants.Pagination.PageSizeTooSmall)
            .LessThanOrEqualTo(ValidationConstants.Pagination.MaxPageSize)
            .WithMessage(ValidationConstants.Pagination.PageSizeTooLarge);
        
        RuleFor(x => x.Filter.SearchTerm)
            .MaximumLength(ValidationConstants.Idea.MaxTitleLength)
            .WithMessage(ValidationConstants.Idea.TitleTooLong)
            .When(x => !string.IsNullOrWhiteSpace(x.Filter.SearchTerm));
        
        RuleFor(x => x.Filter.Status)
            .IsInEnum()
            .WithMessage(ValidationConstants.Idea.InvalidStatus)
            .When(x => x.Filter.Status.HasValue);
    }
}
