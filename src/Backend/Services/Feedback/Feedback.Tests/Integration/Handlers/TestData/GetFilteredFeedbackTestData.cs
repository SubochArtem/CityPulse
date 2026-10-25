using Xunit;

namespace Feedback.Tests.Integration.Handlers.TestData;

public static class GetFilteredFeedbackTestData
{
    public static TheoryData<string> MatchingSearchTerms => new()
    {
        "great",
        "GREAT",
        "GrEaT",
        "  great  ",
        "eat idea"
    };

    public static TheoryData<string> IgnoredSearchTerms => new()
    {
        "",
        "   ",
        "\t"
    };
}
