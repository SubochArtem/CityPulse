using Xunit;

namespace Users.Tests.Integration.Users.TestData;

public static class GetUserByIdTestData
{
    public static TheoryData<string> InvalidGuids => new()
    {
        "not-a-guid",
        "123",
        "00000000-0000-0000-0000-00000000000g"
    };
}
