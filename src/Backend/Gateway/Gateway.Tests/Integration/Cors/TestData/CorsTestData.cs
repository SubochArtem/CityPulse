using Gateway.Tests.Infrastructure.Constants;
using Xunit;

namespace Gateway.Tests.Integration.Cors.TestData;

public static class CorsTestData
{
    public static TheoryData<string> AllowedOrigins => new()
    {
        TestConstants.Origins.Allowed,
        TestConstants.Origins.AnotherAllowed
    };
}
