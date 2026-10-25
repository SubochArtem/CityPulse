using ErrorOr;
using Feedback.Tests.Unit.Errors.TestData;
using Xunit;

namespace Feedback.Tests.Unit.Errors.Tests;

public sealed class FeedbackErrorsTests
{
    private const string CodePrefix = "Feedback.";

    [Theory]
    [MemberData(nameof(FeedbackErrorsTestData.ErrorDefinitions), MemberType = typeof(FeedbackErrorsTestData))]
    public void Error_Always_HasExpectedTypeAndCode(Error error, ErrorType expectedType, string expectedCode)
    {
        Assert.Equal(expectedType, error.Type);
        Assert.Equal(expectedCode, error.Code);
    }

    [Theory]
    [MemberData(nameof(FeedbackErrorsTestData.EachError), MemberType = typeof(FeedbackErrorsTestData))]
    public void Error_Always_HasDescriptionAndFeedbackCodePrefix(Error error)
    {
        Assert.False(string.IsNullOrWhiteSpace(error.Description));
        Assert.StartsWith(CodePrefix, error.Code);
    }

    [Fact]
    public void Errors_Always_HaveUniqueCodes()
    {
        var codes = FeedbackErrorsTestData.AllErrors.Select(error => error.Code).ToList();

        Assert.Equal(codes.Count, codes.Distinct().Count());
    }
}
