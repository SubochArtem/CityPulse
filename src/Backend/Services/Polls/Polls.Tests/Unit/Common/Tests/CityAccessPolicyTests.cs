using Polls.Application.Common.Security;
using Polls.Domain.Polls;
using Xunit;

namespace Polls.Tests.Unit.Common.Tests;

public sealed class CityAccessPolicyTests
{
    [Fact]
    public void Check_UserBelongsToTargetCity_ReturnsSuccess()
    {
        var cityId = Guid.NewGuid();

        var result = CityAccessPolicy.Check(cityId, cityId);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Check_UserBelongsToAnotherCity_ReturnsNotFromUserCityError()
    {
        var targetCityId = Guid.NewGuid();

        var result = CityAccessPolicy.Check(Guid.NewGuid(), targetCityId);

        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.NotFromUserCity(targetCityId), result.Error);
    }

    [Fact]
    public void Check_UserCityIsNotSet_ReturnsNotFromUserCityError()
    {
        var targetCityId = Guid.NewGuid();

        var result = CityAccessPolicy.Check(Guid.Empty, targetCityId);

        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.NotFromUserCity(targetCityId), result.Error);
    }
}
