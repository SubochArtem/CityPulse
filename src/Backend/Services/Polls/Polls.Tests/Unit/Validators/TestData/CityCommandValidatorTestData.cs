using Polls.Application.Common.Constants;
using Xunit;

namespace Polls.Tests.Unit.Validators.TestData;

public static class CityCommandValidatorTestData
{
    public static TheoryData<string?> EmptyTitles => new()
    {
        null,
        "",
        "   "
    };

    public static TheoryData<string> TooLongTitles => new()
    {
        new string('a', ValidationConstants.City.MaxTitleLength + 1),
        new string('a', ValidationConstants.City.MaxTitleLength * 2)
    };

    public static TheoryData<string> ValidTitles => new()
    {
        "a",
        "Warsaw",
        new string('a', ValidationConstants.City.MaxTitleLength)
    };

    public static TheoryData<string> TooLongDescriptions => new()
    {
        new string('a', ValidationConstants.City.MaxDescriptionLength + 1),
        new string('a', ValidationConstants.City.MaxDescriptionLength * 2)
    };

    public static TheoryData<string?> ValidDescriptions => new()
    {
        null,
        "",
        "Capital of Poland",
        new string('a', ValidationConstants.City.MaxDescriptionLength)
    };
}
