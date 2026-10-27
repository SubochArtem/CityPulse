using Xunit;

namespace Gateway.Tests.Unit.Configuration.TestData;

public static class RateLimitingSettingsTestData
{
    public static TheoryData<int> NonPositiveValues => new()
    {
        0,
        -1,
        int.MinValue
    };

    public static TheoryData<int> NegativeValues => new()
    {
        -1,
        int.MinValue
    };
}
