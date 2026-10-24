using FluentValidation;
using Polls.Application.Common.Constants;
using Polls.Application.Ideas.Commands.ChangeApprovalStatus;
using Polls.Domain.Ideas.Enums;

namespace Polls.Tests.Unit.Validators.Tests;

public sealed class ChangeIdeaApprovalStatusCommandValidatorTests
    : ChangeIdeaStatusValidatorTestsBase<ChangeIdeaApprovalStatusCommand, IdeaApprovalStatus>
{
    protected override IValidator<ChangeIdeaApprovalStatusCommand> Validator { get; } =
        new ChangeIdeaApprovalStatusCommandValidator();

    protected override string StatusProperty => nameof(ChangeIdeaApprovalStatusCommand.NewApprovalStatus);

    protected override string InvalidStatusMessage => ValidationConstants.Idea.InvalidApprovalStatus;

    protected override ChangeIdeaApprovalStatusCommand Build(Guid id, IdeaApprovalStatus status) => new(id, status);
}
