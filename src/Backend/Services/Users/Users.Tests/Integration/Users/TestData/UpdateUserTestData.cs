using Xunit;

namespace Users.Tests.Integration.Users.TestData;

public static class UpdateUserTestData
{
    public const string NewNickname = "new_nickname";

    public static TheoryData<string> EmptyUpdateBodies => new()
    {
        "{}",
        """{"nickname":null}""",
        """{"cityId":null}""",
        """{"nickname":null,"cityId":null}"""
    };
}
