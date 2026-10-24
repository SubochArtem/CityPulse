using Polls.Application.Common.Constants;
using Xunit;

namespace Polls.Tests.Unit.Validators.TestData;

public static class IdeaCommandValidatorTestData
{
    public static TheoryData<string?> EmptyTitles => new()
    {
        null,
        "",
        "   "
    };

    public static TheoryData<string> TooLongTitles => new()
    {
        new string('a', ValidationConstants.Idea.MaxTitleLength + 1),
        new string('a', ValidationConstants.Idea.MaxTitleLength * 2)
    };

    public static TheoryData<string> ValidTitles => new()
    {
        "a",
        "Park renovation",
        new string('a', ValidationConstants.Idea.MaxTitleLength)
    };

    public static TheoryData<string> TooLongDescriptions => new()
    {
        new string('a', ValidationConstants.Idea.MaxDescriptionLength + 1),
        new string('a', ValidationConstants.Idea.MaxDescriptionLength * 2)
    };

    public static TheoryData<string?> ValidDescriptions => new()
    {
        null,
        "",
        "Plant more trees in the central park",
        new string('a', ValidationConstants.Idea.MaxDescriptionLength)
    };
}
