using ErrorOr;
using Feedback.API.Common.Errors;
using Xunit;

namespace Feedback.Tests.Unit.Errors.TestData;

public static class FeedbackErrorsTestData
{
    public static readonly Error[] AllErrors =
    [
        FeedbackErrors.NotFound,
        FeedbackErrors.LikeAlreadyExists,
        FeedbackErrors.NotAComment,
        FeedbackErrors.CannotEditOthers,
        FeedbackErrors.CannotDeleteOthers,
        FeedbackErrors.IdeaNotFound,
        FeedbackErrors.NotFromUserCity,
        FeedbackErrors.IdeasServiceUnavailable
    ];

    public static TheoryData<Error> EachError
    {
        get
        {
            var data = new TheoryData<Error>();

            foreach (var error in AllErrors)
                data.Add(error);

            return data;
        }
    }

    public static TheoryData<Error, ErrorType, string> ErrorDefinitions => new()
    {
        { FeedbackErrors.NotFound, ErrorType.NotFound, "Feedback.NotFound" },
        { FeedbackErrors.LikeAlreadyExists, ErrorType.Conflict, "Feedback.LikeAlreadyExists" },
        { FeedbackErrors.NotAComment, ErrorType.Validation, "Feedback.NotAComment" },
        { FeedbackErrors.CannotEditOthers, ErrorType.Forbidden, "Feedback.CannotEditOthers" },
        { FeedbackErrors.CannotDeleteOthers, ErrorType.Forbidden, "Feedback.CannotDeleteOthers" },
        { FeedbackErrors.IdeaNotFound, ErrorType.NotFound, "Feedback.IdeaNotFound" },
        { FeedbackErrors.NotFromUserCity, ErrorType.Forbidden, "Feedback.NotFromUserCity" },
        { FeedbackErrors.IdeasServiceUnavailable, ErrorType.Failure, "Feedback.IdeasServiceUnavailable" }
    };
}
