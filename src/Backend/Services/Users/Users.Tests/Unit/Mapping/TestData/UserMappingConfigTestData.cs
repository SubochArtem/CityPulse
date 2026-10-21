using Users.DataAccess.Entities.Enums;
using Xunit;

namespace Users.Tests.Unit.Mapping.TestData;

public static class UserMappingConfigTestData
{
    public static TheoryData<UserAccessStatus> AccessStatuses => new()
    {
        UserAccessStatus.Undefined,
        UserAccessStatus.Active,
        UserAccessStatus.Inactive,
        UserAccessStatus.Suspended
    };
}
