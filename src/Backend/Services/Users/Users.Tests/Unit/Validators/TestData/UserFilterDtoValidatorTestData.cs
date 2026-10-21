using Users.Business.Constants;
using Xunit;

namespace Users.Tests.Unit.Validators.TestData;

public static class UserFilterDtoValidatorTestData
{
    public static TheoryData<int> InvalidPages => new()
    {
        0,
        -1,
        int.MinValue
    };

    public static TheoryData<int> ValidPages => new()
    {
        ValidationConstants.Pagination.MinPage,
        ValidationConstants.Pagination.MinPage + 1,
        1000
    };

    public static TheoryData<int> TooSmallPageSizes => new()
    {
        0,
        -1
    };

    public static TheoryData<int> TooLargePageSizes => new()
    {
        ValidationConstants.Pagination.MaxPageSize + 1,
        int.MaxValue
    };

    public static TheoryData<int> ValidPageSizes => new()
    {
        ValidationConstants.Pagination.MinPageSize,
        ValidationConstants.Pagination.MaxPageSize,
        50
    };
}
