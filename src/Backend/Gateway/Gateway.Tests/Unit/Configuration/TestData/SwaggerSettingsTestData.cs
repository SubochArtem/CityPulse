using Xunit;

namespace Gateway.Tests.Unit.Configuration.TestData;

public static class SwaggerSettingsTestData
{
    public static TheoryData<string> BlankValues => new()
    {
        string.Empty,
        " ",
        "   "
    };
}
