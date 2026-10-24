using FluentValidation;
using FluentValidation.TestHelper;
using Polls.Application.Common.Constants;
using Polls.Application.Ideas.Commands.CreateIdea;
using Xunit;

namespace Polls.Tests.Unit.Validators.Tests;

public sealed class CreateIdeaCommandValidatorTests : IdeaCommandValidatorTestsBase<CreateIdeaCommand>
{
    private const string DefaultTitle = "Park renovation";

    protected override IValidator<CreateIdeaCommand> Validator { get; } = new CreateIdeaCommandValidator();

    protected override CreateIdeaCommand Build(string title, string? description) =>
        new(Guid.NewGuid(), Guid.NewGuid(), title, description);

    [Fact]
    public void Validate_PollIdIsEmpty_ShouldHaveValidationError()
    {
        var command = new CreateIdeaCommand(Guid.NewGuid(), Guid.Empty, DefaultTitle, null);

        var result = Validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PollId)
            .WithErrorMessage(ValidationConstants.Poll.IdRequired);
    }

    [Fact]
    public void Validate_PollIdIsProvided_ShouldNotHaveValidationError()
    {
        var command = new CreateIdeaCommand(Guid.NewGuid(), Guid.NewGuid(), DefaultTitle, null);

        var result = Validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.PollId);
    }

    [Fact]
    public void Validate_UserIdIsEmpty_ShouldHaveValidationError()
    {
        var command = new CreateIdeaCommand(Guid.Empty, Guid.NewGuid(), DefaultTitle, null);

        var result = Validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(ValidationConstants.Idea.UserIdRequired);
    }

    [Fact]
    public void Validate_UserIdIsProvided_ShouldNotHaveValidationError()
    {
        var command = new CreateIdeaCommand(Guid.NewGuid(), Guid.NewGuid(), DefaultTitle, null);

        var result = Validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }
}
