using Users.DataAccess.Entities.Enums;
using Xunit;

namespace Users.Tests.Integration.Users.TestData;

public static class ActivateUserTestData
{
    public static TheoryData<UserAccessStatus> StatusesThatCanBeActivated => new()
    {
        UserAccessStatus.Inactive,
        UserAccessStatus.Undefined,
        UserAccessStatus.Suspended
    };
}
