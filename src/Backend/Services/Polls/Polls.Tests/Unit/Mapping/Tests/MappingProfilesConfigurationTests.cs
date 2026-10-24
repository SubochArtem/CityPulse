using Polls.Tests.Infrastructure;
using Xunit;

namespace Polls.Tests.Unit.Mapping.Tests;

public sealed class MappingProfilesConfigurationTests
{
    [Fact]
    public void Configuration_ApplicationProfiles_AreValid()
    {
        var configuration = TestMapper.CreateConfiguration();

        configuration.AssertConfigurationIsValid();
    }
}
