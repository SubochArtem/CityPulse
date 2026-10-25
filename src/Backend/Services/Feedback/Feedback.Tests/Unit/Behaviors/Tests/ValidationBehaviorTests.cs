using ErrorOr;
using Feedback.API.Common.Behaviors;
using FluentValidation;
using MediatR;
using Xunit;

namespace Feedback.Tests.Unit.Behaviors.Tests;

public sealed class ValidationBehaviorTests
{
    private const string NameProperty = "Name";
    private const string NameRequiredMessage = "Name is required";
    private const string NameTooShortMessage = "Name is too short";
    private const string NameTooLongMessage = "Name is too long";
    private const string ValidName = "abcd";
    private const string ShortName = "ab";
    private const string EmptyName = "";
    private const string SuccessValue = "handled";
    private const int MinNameLength = 3;
    private const int MaxNameLength = 5;
    private const string InvalidResultMessage = "is not a valid ErrorOr<T> for validation failures";

    private interface IHasName
    {
        string Name { get; }
    }

    private sealed record NameRequest(string Name) : IRequest<ErrorOr<string>>, IHasName;

    private sealed record NameCommand(string Name) : IRequest<ErrorOr<Success>>, IHasName;

    private sealed record NumberRequest(string Name) : IRequest<ErrorOr<int>>, IHasName;

    private sealed record PlainRequest(string Name) : IRequest<IErrorOr>, IHasName;

    private sealed class NameRequiredValidator<T> : AbstractValidator<T> where T : IHasName
    {
        public NameRequiredValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(NameRequiredMessage);
        }
    }

    private sealed class NameMinLengthValidator<T> : AbstractValidator<T> where T : IHasName
    {
        public NameMinLengthValidator()
        {
            RuleFor(x => x.Name).MinimumLength(MinNameLength).WithMessage(NameTooShortMessage);
        }
    }

    private sealed class NameMaxLengthValidator<T> : AbstractValidator<T> where T : IHasName
    {
        public NameMaxLengthValidator()
        {
            RuleFor(x => x.Name).MaximumLength(MaxNameLength).WithMessage(NameTooLongMessage);
        }
    }

    private static ValidationBehavior<NameRequest, ErrorOr<string>> BehaviorFor(
        params IValidator<NameRequest>[] validators) =>
        new(validators);

    private static Error ValidationError(string message) => Error.Validation(NameProperty, message);

    private static ErrorOr<string> SuccessResponse() => SuccessValue;

    [Fact]
    public async Task Handle_NoValidators_CallsNextAndReturnsItsResult()
    {
        ErrorOr<string> expected = SuccessValue;
        var calls = 0;

        var result = await BehaviorFor().Handle(
            new NameRequest(EmptyName),
            _ =>
            {
                calls++;
                return Task.FromResult(expected);
            },
            CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(SuccessValue, result.Value);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task Handle_AllValidatorsPass_CallsNextOnceAndReturnsItsResult()
    {
        ErrorOr<string> expected = SuccessValue;
        var calls = 0;
        var behavior = BehaviorFor(
            new NameRequiredValidator<NameRequest>(),
            new NameMinLengthValidator<NameRequest>(),
            new NameMaxLengthValidator<NameRequest>());

        var result = await behavior.Handle(
            new NameRequest(ValidName),
            _ =>
            {
                calls++;
                return Task.FromResult(expected);
            },
            CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(SuccessValue, result.Value);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task Handle_ValidationPasses_PassesCancellationTokenToNext()
    {
        using var cts = new CancellationTokenSource();
        CancellationToken? received = null;
        var behavior = BehaviorFor(new NameRequiredValidator<NameRequest>());

        await behavior.Handle(
            new NameRequest(ValidName),
            token =>
            {
                received = token;
                return Task.FromResult<ErrorOr<string>>(SuccessValue);
            },
            cts.Token);

        Assert.Equal(cts.Token, received);
    }

    [Fact]
    public async Task Handle_ValidatorFails_DoesNotCallNextAndReturnsValidationErrorWithPropertyAsCode()
    {
        var calls = 0;
        var behavior = BehaviorFor(new NameRequiredValidator<NameRequest>());

        var result = await behavior.Handle(
            new NameRequest(EmptyName),
            _ =>
            {
                calls++;
                return Task.FromResult<ErrorOr<string>>(SuccessValue);
            },
            CancellationToken.None);

        Assert.Equal(0, calls);
        Assert.True(result.IsError);
        Assert.Equal([ValidationError(NameRequiredMessage)], result.Errors);
    }

    [Fact]
    public async Task Handle_SeveralValidatorsFail_ReturnsAllErrorsInValidatorOrder()
    {
        var behavior = BehaviorFor(
            new NameRequiredValidator<NameRequest>(),
            new NameMinLengthValidator<NameRequest>());

        var result = await behavior.Handle(
            new NameRequest(EmptyName),
            _ => Task.FromResult<ErrorOr<string>>(SuccessValue),
            CancellationToken.None);

        Assert.Equal(
            [ValidationError(NameRequiredMessage), ValidationError(NameTooShortMessage)],
            result.Errors);
    }

    [Fact]
    public async Task Handle_OneValidatorPassesAndAnotherFails_ReturnsOnlyFailingValidatorErrors()
    {
        var behavior = BehaviorFor(
            new NameMaxLengthValidator<NameRequest>(),
            new NameMinLengthValidator<NameRequest>());

        var result = await behavior.Handle(
            new NameRequest(ShortName),
            _ => Task.FromResult<ErrorOr<string>>(SuccessValue),
            CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal([ValidationError(NameTooShortMessage)], result.Errors);
    }

    [Fact]
    public async Task Handle_ValidationFails_ErrorsHaveValidationType()
    {
        var behavior = BehaviorFor(
            new NameRequiredValidator<NameRequest>(),
            new NameMinLengthValidator<NameRequest>());

        var result = await behavior.Handle(
            new NameRequest(EmptyName),
            _ => Task.FromResult<ErrorOr<string>>(SuccessValue),
            CancellationToken.None);

        Assert.All(result.Errors, error => Assert.Equal(ErrorType.Validation, error.Type));
    }

    [Fact]
    public async Task Handle_ValueTypeResponseFails_CreatesFailedResult()
    {
        var behavior = new ValidationBehavior<NumberRequest, ErrorOr<int>>(
            [new NameRequiredValidator<NumberRequest>()]);

        var result = await behavior.Handle(
            new NumberRequest(EmptyName),
            _ => Task.FromResult<ErrorOr<int>>(1),
            CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal([ValidationError(NameRequiredMessage)], result.Errors);
    }

    [Fact]
    public async Task Handle_SuccessResponseFails_CreatesFailedResult()
    {
        var behavior = new ValidationBehavior<NameCommand, ErrorOr<Success>>(
            [new NameRequiredValidator<NameCommand>()]);

        var result = await behavior.Handle(
            new NameCommand(EmptyName),
            _ => Task.FromResult<ErrorOr<Success>>(Result.Success),
            CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal([ValidationError(NameRequiredMessage)], result.Errors);
    }

    [Fact]
    public async Task Handle_ResponseIsNotGenericErrorOr_ThrowsConfigurationError()
    {
        var behavior = new ValidationBehavior<PlainRequest, IErrorOr>(
            [new NameRequiredValidator<PlainRequest>()]);

        var exception = await Assert.ThrowsAsync<TypeInitializationException>(
            () => behavior.Handle(
                new PlainRequest(EmptyName),
                _ => Task.FromResult<IErrorOr>(SuccessResponse()),
                CancellationToken.None));

        var inner = Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Contains(InvalidResultMessage, inner.Message);
    }
}
