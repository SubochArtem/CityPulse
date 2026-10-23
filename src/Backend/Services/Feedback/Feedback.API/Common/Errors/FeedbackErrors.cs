using ErrorOr;

namespace Feedback.API.Common.Errors;

public static class FeedbackErrors
{
    public static readonly Error NotFound = Error.NotFound(
        code: "Feedback.NotFound",
        description: "Feedback was not found.");

    public static readonly Error LikeAlreadyExists = Error.Conflict(
        code: "Feedback.LikeAlreadyExists",
        description: "Like already exists.");

    public static readonly Error NotAComment = Error.Validation(
        code: "Feedback.NotAComment",
        description: "Only comments can be edited.");

    public static readonly Error CannotEditOthers = Error.Forbidden(
        code: "Feedback.CannotEditOthers",
        description: "You can edit only your own comments.");

    public static readonly Error CannotDeleteOthers = Error.Forbidden(
        code: "Feedback.CannotDeleteOthers",
        description: "You can delete only your own feedback.");

    public static readonly Error IdeaNotFound = Error.NotFound(
        code: "Feedback.IdeaNotFound",
        description: "Idea was not found.");

    public static readonly Error NotFromUserCity = Error.Forbidden(
        code: "Feedback.NotFromUserCity",
        description: "You can leave feedback only on ideas from your city.");

    public static readonly Error IdeasServiceUnavailable = Error.Failure(
        code: "Feedback.IdeasServiceUnavailable",
        description: "Ideas service is currently unavailable.");
}
