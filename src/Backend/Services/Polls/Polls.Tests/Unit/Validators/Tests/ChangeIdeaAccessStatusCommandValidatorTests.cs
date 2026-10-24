using FluentValidation;
using Polls.Application.Common.Constants;
using Polls.Application.Ideas.Commands.ChangeAccessStatus;
using Polls.Domain.Ideas.Enums;

namespace Polls.Tests.Unit.Validators.Tests;

public sealed class ChangeIdeaAccessStatusCommandValidatorTests
    : ChangeStatusValidatorTestsBase<ChangeIdeaAccessStatusCommand, IdeaAccessStatus>
{
    protected override IValidator<ChangeIdeaAccessStatusCommand> Validator { get; } =
        new ChangeIdeaAccessStatusCommandValidator();

    protected override string IdRequiredMessage => ValidationConstants.Idea.IdRequired;

    protected override string StatusProperty => nameof(ChangeIdeaAccessStatusCommand.NewIdeaAccessStatus);

    protected override string InvalidStatusMessage => ValidationConstants.Idea.InvalidAccessStatus;

    protected override ChangeIdeaAccessStatusCommand Build(Guid id, IdeaAccessStatus status) => new(id, status);
}
