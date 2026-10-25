using ErrorOr;
using Feedback.API.Common.Errors;
using Xunit;

namespace Feedback.Tests.Integration.Handlers.TestData;

public static class AddFeedbackTestData
{
    public static TheoryData<Error> IdeaServiceErrors => new()
    {
        FeedbackErrors.IdeaNotFound,
        FeedbackErrors.IdeasServiceUnavailable
    };

    public static TheoryData<bool> BypassRestrictionsValues => new()
    {
        false,
        true
    };
}
