using Users.Business.Constants;
using Xunit;

namespace Users.Tests.Unit.Validators.TestData;

public static class UpdateUserProfileDtoValidatorTestData
{
    public static TheoryData<string> EmptyNicknames => new()
    {
        "",
        "   "
    };

    public static TheoryData<string> TooShortNicknames => new()
    {
        "a",
        "aa"
    };

    public static TheoryData<string> InvalidCharacterNicknames => new()
    {
        "abc def",
        "abc!def",
        "abc#def",
        "abc@def",
        "abc/def"
    };

    public static TheoryData<string> ConsecutiveSpecialCharacterNicknames => new()
    {
        "abc..def",
        "abc__def",
        "abc++def",
        "abc--def",
        "abc.-def",
        "abc_+def"
    };

    public static TheoryData<string> ValidNicknames => new()
    {
        "abc",
        "abc_def",
        "abc.def",
        "abc-def",
        "abc+def",
        "abcdef123",
        new string('a', ValidationConstants.Nickname.MaxLength)
    };
}
