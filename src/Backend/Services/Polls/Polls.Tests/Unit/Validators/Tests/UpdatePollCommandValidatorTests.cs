using FluentValidation;
using FluentValidation.TestHelper;
using Polls.Application.Common.Constants;
using Polls.Application.Polls.Commands.UpdatePoll;
using Xunit;

namespace Polls.Tests.Unit.Validators.Tests;

public sealed class UpdatePollCommandValidatorTests : PollCommandValidatorTestsBase<UpdatePollCommand>
{
    private const string DefaultTitle = "Budget for the new playground";
    private const decimal DefaultBudget = 1000;
    private const int OneYearDays = 365;

    protected override IValidator<UpdatePollCommand> Validator { get; } = new UpdatePollCommandValidator();

    protected override UpdatePollCommand Build(
        string title,
        string? description,
        decimal budgetAmount,
        DateTimeOffset endsAt) =>
        new(Guid.NewGuid(), title, description, endsAt, budgetAmount);

    [Fact]
    public void Validate_IdIsEmpty_ShouldHaveValidationError()
    {
        var command = new UpdatePollCommand(Guid.Empty, DefaultTitle, null, ValidEndsAt(), DefaultBudget);

        var result = Validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage(ValidationConstants.Poll.IdRequired);
    }

    [Fact]
    public void Validate_IdIsProvided_ShouldNotHaveValidationError()
    {
        var command = new UpdatePollCommand(Guid.NewGuid(), DefaultTitle, null, ValidEndsAt(), DefaultBudget);

        var result = Validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validate_EndsAtIsFarInTheFuture_ShouldNotHaveValidationError()
    {
        var endsAt = DateTimeOffset.UtcNow.AddDays(OneYearDays);
        var command = new UpdatePollCommand(Guid.NewGuid(), DefaultTitle, null, endsAt, DefaultBudget);

        var result = Validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.EndsAt);
    }
}
