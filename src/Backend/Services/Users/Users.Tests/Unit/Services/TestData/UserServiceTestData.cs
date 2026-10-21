using Users.DataAccess.Entities.Enums;
using Xunit;

namespace Users.Tests.Unit.Services.TestData;

public static class UserServiceTestData
{
    public static TheoryData<UserAccessStatus> NonInactiveStatuses => new()
    {
        UserAccessStatus.Active
    };
}
