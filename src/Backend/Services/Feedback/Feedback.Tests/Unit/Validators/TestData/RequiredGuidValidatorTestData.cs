using Feedback.API.Features.Feedback.AddFeedback;
using Feedback.API.Features.Feedback.DeleteFeedback;
using Feedback.API.Features.Feedback.EditComment;
using FluentValidation;
using Xunit;

namespace Feedback.Tests.Unit.Validators.TestData;

public sealed record RequiredGuidCase(
    string Name,
    IValidator Validator,
    Func<Guid, object> BuildCommand,
    string PropertyName,
    string ExpectedMessage)
{
    public override string ToString() => Name;
}

public static class RequiredGuidValidatorTestData
{
    private const string IdeaIdMessage = "Idea id is required.";
    private const string UserIdMessage = "User id is required.";
    private const string FeedbackIdMessage = "Feedback id is required.";
    private const string CommentText = "Comment text";

    private static readonly Guid ValidId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static TheoryData<RequiredGuidCase> Cases => new()
    {
        new RequiredGuidCase(
            "AddFeedback.IdeaId",
            new AddFeedbackCommandValidator(),
            id => new AddFeedbackCommand(ValidId, ValidId, id, null, false),
            nameof(AddFeedbackCommand.IdeaId),
            IdeaIdMessage),
        new RequiredGuidCase(
            "AddFeedback.UserId",
            new AddFeedbackCommandValidator(),
            id => new AddFeedbackCommand(id, ValidId, ValidId, null, false),
            nameof(AddFeedbackCommand.UserId),
            UserIdMessage),
        new RequiredGuidCase(
            "EditComment.Id",
            new EditCommentCommandValidator(),
            id => new EditCommentCommand(ValidId, id, CommentText, false),
            nameof(EditCommentCommand.Id),
            FeedbackIdMessage),
        new RequiredGuidCase(
            "EditComment.UserId",
            new EditCommentCommandValidator(),
            id => new EditCommentCommand(id, ValidId, CommentText, false),
            nameof(EditCommentCommand.UserId),
            UserIdMessage),
        new RequiredGuidCase(
            "DeleteFeedback.Id",
            new DeleteFeedbackCommandValidator(),
            id => new DeleteFeedbackCommand(ValidId, id, false),
            nameof(DeleteFeedbackCommand.Id),
            FeedbackIdMessage),
        new RequiredGuidCase(
            "DeleteFeedback.UserId",
            new DeleteFeedbackCommandValidator(),
            id => new DeleteFeedbackCommand(id, ValidId, false),
            nameof(DeleteFeedbackCommand.UserId),
            UserIdMessage)
    };
}
