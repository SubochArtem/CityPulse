using Gateway.Tests.Infrastructure.Constants;
using Xunit;

namespace Gateway.Tests.Integration.Swagger.TestData;

public static class SwaggerTestData
{
    public static TheoryData<string> Services => new()
    {
        TestConstants.Services.Users,
        TestConstants.Services.Polls,
        TestConstants.Services.Feedback
    };

    public static TheoryData<string> DocumentTitles => new()
    {
        TestConstants.Swagger.UsersTitle,
        TestConstants.Swagger.PollsTitle,
        TestConstants.Swagger.FeedbackTitle
    };
}
