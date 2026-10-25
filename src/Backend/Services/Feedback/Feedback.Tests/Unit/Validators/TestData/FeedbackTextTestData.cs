using Xunit;

namespace Feedback.Tests.Unit.Validators.TestData;

public static class FeedbackTextTestData
{
    private const int MaxTextLength = 1000;

    public static TheoryData<string> EmptyTexts => new()
    {
        "",
        "   ",
        "\t"
    };

    public static TheoryData<string> TooLongTexts => new()
    {
        new string('a', MaxTextLength + 1),
        new string('a', MaxTextLength * 2)
    };

    public static TheoryData<string> ValidTexts => new()
    {
        "a",
        "Great idea!",
        new string('a', MaxTextLength)
    };
}
