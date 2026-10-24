using FluentValidation;
using FluentValidation.TestHelper;
using Polls.Application.Common.Constants;
using Polls.Application.Polls.Commands.CreatePoll;
using Polls.Domain.Polls.Enums;
using Polls.Tests.Unit.Validators.TestData;
using Xunit;

namespace Polls.Tests.Unit.Validators.Tests;

public sealed class CreatePollCommandValidatorTests : PollCommandValidatorTestsBase<CreatePollCommand>
{
    private const string DefaultTitle = "Budget for the new playground";
    private const decimal DefaultBudget = 1000;

    protected override IValidator<CreatePollCommand> Validator { get; } = new CreatePollCommandValidator();

    protected override CreatePollCommand Build(
        string title,
        string? description,
        decimal budgetAmount,
        DateTimeOffset endsAt) =>
        new(Guid.NewGuid(), title, description, PollType.UserVote, endsAt, budgetAmount);

    private static CreatePollCommand BuildWith(
        Guid? cityId = null,
        PollType type = PollType.UserVote,
        DateTimeOffset? endsAt = null) =>
        new(cityId ?? Guid.NewGuid(), DefaultTitle, null, type, endsAt ?? ValidEndsAt(), DefaultBudget);

    [Fact]
    public void Validate_CityIdIsEmpty_ShouldHaveValidationError()
    {
        var result = Validator.TestValidate(BuildWith(cityId: Guid.Empty));

        result.ShouldHaveValidationErrorFor(x => x.CityId)
            .WithErrorMessage(ValidationConstants.City.IdRequired);
    }

    [Fact]
    public void Validate_CityIdIsProvided_ShouldNotHaveValidationError()
    {
        var result = Validator.TestValidate(BuildWith(cityId: Guid.NewGuid()));

        result.ShouldNotHaveValidationErrorFor(x => x.CityId);
    }

    [Theory]
    [MemberData(nameof(CreatePollCommandValidatorTestData.UnknownTypes), MemberType = typeof(CreatePollCommandValidatorTestData))]
    public void Validate_TypeIsNotDefinedInEnum_ShouldHaveValidationError(PollType type)
    {
        var result = Validator.TestValidate(BuildWith(type: type));

        result.ShouldHaveValidationErrorFor(x => x.Type)
            .WithErrorMessage(ValidationConstants.Poll.InvalidType);
    }

    [Theory]
    [MemberData(nameof(CreatePollCommandValidatorTestData.DefinedTypes), MemberType = typeof(CreatePollCommandValidatorTestData))]
    public void Validate_TypeIsDefinedInEnum_ShouldNotHaveValidationError(PollType type)
    {
        var result = Validator.TestValidate(BuildWith(type: type));

        result.ShouldNotHaveValidationErrorFor(x => x.Type);
    }

    [Theory]
    [MemberData(nameof(CreatePollCommandValidatorTestData.TooShortDurations), MemberType = typeof(CreatePollCommandValidatorTestData))]
    public void Validate_DurationIsTooShort_ShouldHaveValidationError(TimeSpan duration)
    {
        var result = Validator.TestValidate(BuildWith(endsAt: DateTimeOffset.UtcNow.Add(duration)));

        result.ShouldHaveValidationErrorFor(x => x.EndsAt)
            .WithErrorMessage(ValidationConstants.Poll.TooShortDuration);
    }

    [Theory]
    [MemberData(nameof(CreatePollCommandValidatorTestData.TooLongDurations), MemberType = typeof(CreatePollCommandValidatorTestData))]
    public void Validate_DurationIsTooLong_ShouldHaveValidationError(TimeSpan duration)
    {
        var result = Validator.TestValidate(BuildWith(endsAt: DateTimeOffset.UtcNow.Add(duration)));

        result.ShouldHaveValidationErrorFor(x => x.EndsAt)
            .WithErrorMessage(ValidationConstants.Poll.TooLongDuration);
    }

    [Theory]
    [MemberData(nameof(CreatePollCommandValidatorTestData.ValidDurations), MemberType = typeof(CreatePollCommandValidatorTestData))]
    public void Validate_DurationIsWithinAllowedRange_ShouldNotHaveValidationError(TimeSpan duration)
    {
        var result = Validator.TestValidate(BuildWith(endsAt: DateTimeOffset.UtcNow.Add(duration)));

        result.ShouldNotHaveValidationErrorFor(x => x.EndsAt);
    }
}
