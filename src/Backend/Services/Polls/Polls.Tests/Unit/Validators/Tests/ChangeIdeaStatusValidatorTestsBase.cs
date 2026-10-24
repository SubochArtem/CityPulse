using FluentValidation;
using FluentValidation.TestHelper;
using Polls.Application.Common.Constants;
using Xunit;

namespace Polls.Tests.Unit.Validators.Tests;

public abstract class ChangeIdeaStatusValidatorTestsBase<TCommand, TStatus>
    where TStatus : struct, Enum
{
    private const string IdProperty = "Id";
    private static readonly int[] UnknownStatusValues = [99, -1];

    protected abstract IValidator<TCommand> Validator { get; }

    protected abstract string StatusProperty { get; }

    protected abstract string InvalidStatusMessage { get; }

    protected abstract TCommand Build(Guid id, TStatus status);

    private static TStatus DefinedStatus => Enum.GetValues<TStatus>()[0];

    [Fact]
    public void Validate_IdIsEmpty_ShouldHaveValidationError()
    {
        var command = Build(Guid.Empty, DefinedStatus);

        var result = Validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(IdProperty)
            .WithErrorMessage(ValidationConstants.Idea.IdRequired);
    }

    [Fact]
    public void Validate_IdIsProvided_ShouldNotHaveValidationError()
    {
        var command = Build(Guid.NewGuid(), DefinedStatus);

        var result = Validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(IdProperty);
    }

    [Fact]
    public void Validate_StatusIsNotDefinedInEnum_ShouldHaveValidationError()
    {
        foreach (var value in UnknownStatusValues)
        {
            var status = (TStatus)Enum.ToObject(typeof(TStatus), value);
            var command = Build(Guid.NewGuid(), status);

            var result = Validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(StatusProperty)
                .WithErrorMessage(InvalidStatusMessage);
        }
    }

    [Fact]
    public void Validate_StatusIsDefinedInEnum_ShouldNotHaveValidationError()
    {
        foreach (var status in Enum.GetValues<TStatus>())
        {
            var command = Build(Guid.NewGuid(), status);

            var result = Validator.TestValidate(command);

            result.ShouldNotHaveValidationErrorFor(StatusProperty);
        }
    }

    [Fact]
    public void Validate_CommandIsValid_ShouldNotHaveAnyValidationErrors()
    {
        var result = Validator.TestValidate(Build(Guid.NewGuid(), DefinedStatus));

        result.ShouldNotHaveAnyValidationErrors();
    }
}
