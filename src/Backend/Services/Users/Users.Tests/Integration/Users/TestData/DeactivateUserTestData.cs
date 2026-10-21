using Users.DataAccess.Entities.Enums;
using Xunit;

namespace Users.Tests.Integration.Users.TestData;

public static class DeactivateUserTestData
{
    public static TheoryData<UserAccessStatus> StatusesThatCanBeDeactivated => new()
    {
        UserAccessStatus.Active,
        UserAccessStatus.Undefined,
        UserAccessStatus.Suspended
    };
}
