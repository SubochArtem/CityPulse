using FluentValidation;
using Polls.Application.Common.Constants;

namespace Polls.Application.Cities.Commands.DeleteCity;

public sealed class DeleteCityCommandValidator : AbstractValidator<DeleteCityCommand>
{
    public DeleteCityCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty()
            .WithMessage(ValidationConstants.City.IdRequired);
    }
}
