using FluentValidation;
using FluentValidation.TestHelper;
using Polls.Application.Common.Constants;
using Polls.Application.Ideas.Commands.UpdateIdea;
using Xunit;

namespace Polls.Tests.Unit.Validators.Tests;

public sealed class UpdateIdeaCommandValidatorTests : IdeaCommandValidatorTestsBase<UpdateIdeaCommand>
{
    private const string DefaultTitle = "Park renovation";

    protected override IValidator<UpdateIdeaCommand> Validator { get; } = new UpdateIdeaCommandValidator();

    protected override UpdateIdeaCommand Build(string title, string? description) =>
        new(Guid.NewGuid(), title, description);

    [Fact]
    public void Validate_IdIsEmpty_ShouldHaveValidationError()
    {
        var command = new UpdateIdeaCommand(Guid.Empty, DefaultTitle, null);

        var result = Validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage(ValidationConstants.Idea.IdRequired);
    }

    [Fact]
    public void Validate_IdIsProvided_ShouldNotHaveValidationError()
    {
        var command = new UpdateIdeaCommand(Guid.NewGuid(), DefaultTitle, null);

        var result = Validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }
}
