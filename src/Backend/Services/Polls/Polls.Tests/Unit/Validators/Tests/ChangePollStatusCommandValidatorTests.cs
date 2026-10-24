using FluentValidation;
using Polls.Application.Common.Constants;
using Polls.Application.Polls.Commands.ChangeStatus;
using Polls.Domain.Polls.Enums;

namespace Polls.Tests.Unit.Validators.Tests;

public sealed class ChangePollStatusCommandValidatorTests
    : ChangeStatusValidatorTestsBase<ChangePollStatusCommand, PollStatus>
{
    protected override IValidator<ChangePollStatusCommand> Validator { get; } =
        new ChangePollStatusCommandValidator();

    protected override string IdRequiredMessage => ValidationConstants.Poll.IdRequired;

    protected override string StatusProperty => nameof(ChangePollStatusCommand.NewStatus);

    protected override string InvalidStatusMessage => ValidationConstants.Poll.InvalidStatus;

    protected override ChangePollStatusCommand Build(Guid id, PollStatus status) => new(id, status);
}
