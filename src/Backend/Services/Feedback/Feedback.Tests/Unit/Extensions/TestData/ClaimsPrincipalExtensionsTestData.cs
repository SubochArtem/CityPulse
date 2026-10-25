using Xunit;

namespace Feedback.Tests.Unit.Extensions.TestData;

public static class ClaimsPrincipalExtensionsTestData
{
    public static TheoryData<string> InvalidCityIdClaims => new()
    {
        "",
        "   ",
        "not-a-guid",
        "12345",
        "00000000-0000-0000-0000"
    };
}
