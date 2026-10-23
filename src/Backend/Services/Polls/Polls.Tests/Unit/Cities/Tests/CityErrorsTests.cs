using Polls.Domain.Cities;
using Polls.Domain.Cities.Enums;
using Polls.Domain.Common.Enums;
using Polls.Tests.Unit.Cities.TestData;
using Xunit;

namespace Polls.Tests.Unit.Cities.Tests;

public sealed class CityErrorsTests
{
    [Fact]
    public void AlreadyExists_Always_ReturnsConflictError()
    {
        var error = CityErrors.AlreadyExists;

        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.False(string.IsNullOrWhiteSpace(error.Description));
    }

    [Fact]
    public void NotFound_WithId_ReturnsNotFoundErrorMentioningId()
    {
        var id = Guid.NewGuid();

        var error = CityErrors.NotFound(id);

        Assert.Equal(ErrorType.NotFound, error.Type);
        Assert.Contains(id.ToString(), error.Description);
    }

    [Theory]
    [MemberData(nameof(CityErrorsTestData.AllStatuses), MemberType = typeof(CityErrorsTestData))]
    public void InvalidStatus_AnyStatus_ReturnsConflictErrorMentioningStatus(CityStatus status)
    {
        var error = CityErrors.InvalidStatus(status);

        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.Contains(status.ToString(), error.Description);
    }
}
