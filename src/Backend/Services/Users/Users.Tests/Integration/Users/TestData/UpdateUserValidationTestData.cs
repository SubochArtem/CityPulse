using Users.Business.Constants;
using Xunit;

namespace Users.Tests.Integration.Users.TestData;

public static class UpdateUserValidationTestData
{
    public const string NicknameKey = "Nickname";

    public static TheoryData<string, string> InvalidNicknames => new()
    {
        { "", ValidationConstants.Nickname.Required },
        { "ab", ValidationConstants.Nickname.TooShort },
        { new string('a', ValidationConstants.Nickname.MaxLength + 1), ValidationConstants.Nickname.TooLong },
        { "bad nick", ValidationConstants.Nickname.InvalidCharacters },
        { "bad!nick", ValidationConstants.Nickname.InvalidCharacters },
        { "a..b", ValidationConstants.Nickname.ConsecutiveSpecialCharacters },
        { "a_-b", ValidationConstants.Nickname.ConsecutiveSpecialCharacters }
    };
}
